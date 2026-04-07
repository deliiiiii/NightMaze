using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace General
{
    public class CompactFormatNoRefConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var previousFormatting = writer.Formatting;
            var previousReferenceHandling = serializer.PreserveReferencesHandling;
            writer.Formatting = Formatting.None;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.None;
            serializer.Serialize(writer, value);
            writer.Formatting = previousFormatting;
            serializer.PreserveReferencesHandling = previousReferenceHandling;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var previousReferenceHandling = serializer.PreserveReferencesHandling;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.None;
            var result = serializer.Deserialize(reader, objectType);
            serializer.PreserveReferencesHandling = previousReferenceHandling;
            return result;
        }
    }
    
    public class CompactFormatRefConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var preFormatting = writer.Formatting;
            var preReferenceHandling = serializer.PreserveReferencesHandling;
            var preTypeNameHandling = serializer.TypeNameHandling;
            writer.Formatting = Formatting.None;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.All;
            serializer.TypeNameHandling = TypeNameHandling.All;
            serializer.Serialize(writer, value);
            writer.Formatting = preFormatting;
            serializer.PreserveReferencesHandling = preReferenceHandling;
            serializer.TypeNameHandling = preTypeNameHandling;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var previousReferenceHandling = serializer.PreserveReferencesHandling;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.All;
            var result = serializer.Deserialize(reader, objectType);
            serializer.PreserveReferencesHandling = previousReferenceHandling;
            return result;
        }
    }
    
    internal class PrivateFieldsContractResolver : DefaultContractResolver
    {
        public static readonly PrivateFieldsContractResolver Instance = new PrivateFieldsContractResolver();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = new List<JsonProperty>(base.CreateProperties(type, memberSerialization));

            // 不仅过滤只读属性, 还必须修正 private set 属性的写入权限
            props.RemoveAll(p =>
            {
                var propInfo = p.DeclaringType?.GetProperty(p.UnderlyingName ?? "",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (propInfo != null)
                {
                    // 1. 如果属性完全不可写 如{ get; }, 则移除
                    if (!propInfo.CanWrite) 
                        return true;
                    // 2. 如果属性有 private set, CanWrite 是 true,
                    // 但 base.CreateProperties 默认会将其 Writable 设为 false.
                    // 必须强制设为 true, 否则反序列化时会直接忽略该字段.
                    p.Writable = true;
                }
                return false;
            });
            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                // A. 补充私有属性
                var nonPublicProps = currentType.GetProperties(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var propInfo in nonPublicProps)
                {
                    // 过滤只读属性
                    if (!propInfo.CanWrite) 
                        continue;
                    var jsonProp = base.CreateProperty(propInfo, memberSerialization);
                    if (jsonProp.Ignored) 
                        continue;
                    if (props.Any(p => p.PropertyName == jsonProp.PropertyName))
                        continue;
                    jsonProp.Writable = true;
                    jsonProp.Readable = true;
                    props.Add(jsonProp);
                }
                // B. 补充私有字段
                var fields = currentType.GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(CompilerGeneratedAttribute)))
                        continue;
                    var jsonProp = base.CreateProperty(field, memberSerialization);
                    if (jsonProp.Ignored)
                        continue;
                    if (props.Any(p => p.PropertyName == jsonProp.PropertyName))
                        continue;
                    jsonProp.Writable = true;
                    jsonProp.Readable = true;
                    props.Add(jsonProp);
                }
                currentType = currentType.BaseType;
            }
            var result = props.Where(p => !typeof(Delegate).IsAssignableFrom(p.PropertyType)).ToList();
            return result.OrderBy(p => GetTypeDepth(p.DeclaringType) + (p.Order ?? 0)).ToList();
        }
        static int GetTypeDepth(Type t)
        {
            int depth = 0;
            while (t != null)
            {
                t = t.BaseType;
                depth++;
            }
            return depth;
        }
    }

    internal static class JsonIO
    {
        static readonly JsonSerializerSettings settings = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = PrivateFieldsContractResolver.Instance,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        };
        public static void Write<T>(string pathPre, string name, T obj)
        {
            //Debug.Log("write"+curEntity);
            string path = pathPre +"/" + name + ".json";
            if (!Directory.Exists(pathPre))
            {
                Directory.CreateDirectory(pathPre);
            }
            // string str = JsonUtility.ToJson(curEntity, true);
            string str = JsonConvert.SerializeObject(obj, settings);
            File.WriteAllText(path, str);
        }
        public static async UniTask<T> ReadAsync<T>(string pathPre, string name, CancellationToken ct)
        {
            string path = pathPre + "/" + name + ".json";
            if (!File.Exists(path))
            {
                MyDebug.Log("path :" + path + " not exist");
                return default;
            }
            string str = await File.ReadAllTextAsync(path, ct);
            using var _ = BusDisposable.MuteScope(typeof(T).Name);
            using var _2 = BusDisposable.MuteScope("Data");
            return JsonConvert.DeserializeObject<T>(str, settings);
        }
        public static async UniTask<T> ReadWithVerAsync<T>(string pathPre, string name, CancellationToken ct)
            where T : IHasVersion
        {
            string path = pathPre + "/" + name + ".json";
            if (!File.Exists(path))
            {
                MyDebug.Log("path :" + path + " not exist");
                return default;
            }
            string str = await File.ReadAllTextAsync(path, ct);
            using var _ = BusDisposable.MuteScope(typeof(T).Name);
            using var _2 = BusDisposable.MuteScope("Data");
            var nullableJObj = MigrateStepFactory<JObject, T>.MigrateUntilCur(JObject.Parse(str));
            return nullableJObj == null 
                ? Activator.CreateInstance<T>() 
                : nullableJObj.ToObject<T>(JsonSerializer.Create(settings));
            // return nullableJObj == null
            //     ? Activator.CreateInstance<T>()
            //     : JsonConvert.DeserializeObject<T>(nullableJObj.ToString(), settings);
        }
        public static void Delete(string pathPre, string name)
        {
            string path = pathPre + "/" + name + ".json";
            File.Delete(path);
        }
        //加密
        public static string StringToByteString(string str)
        {
            return EncryptDES(Convert.ToBase64String(Encoding.UTF8.GetBytes(str)));
        }

        //解密
        public static string ByteStringToString(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(DecryptDES(str)));
        }

        #region  字符串加密解密
        static readonly byte[] keys = { 0x20, 0x05, 0x85, 0x74, 0x96, 0xA1, 0xB2, 0xC3 };
        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="key">加密密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        static string EncryptDES(string encryptString, string key = "13717421")
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                byte[] rgbIV = keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                cStream.Close();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                //Debug.LogError("StringEncrypt/EncryptDES()/ Encrypt error!");
                return encryptString;
            }
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="key">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        static string DecryptDES(string decryptString, string key = "13717421")
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(key);
                byte[] rgbIV = keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                var dcsp = new DESCryptoServiceProvider();
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, dcsp.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                cStream.Close();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                //Debug.LogError("StringEncrypt/DecryptDES()/ Decrypt error!");
                return decryptString;
            }
        }
        #endregion
    }
    
}