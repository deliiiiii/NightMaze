using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

class PrivateFieldsContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        // 1. 只获取带有 [JsonProperty] 特性的属性
        var props = base.CreateProperties(type, memberSerialization)
            .Where(p => p.AttributeProvider.GetAttributes(typeof(JsonPropertyAttribute), true).Any())
            .ToList();

        // 2. 遍历类型层次结构以包含所有基类的字段
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            // 获取当前类型的所有实例字段
            var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                // 排除委托类型的字段
                if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                // 避免重复添加（例如，如果字段是已添加属性的后备字段）
                if (props.All(p => p.UnderlyingName != field.Name))
                {
                    var prop = CreateProperty(field, memberSerialization);
                    prop.Readable = true;
                    prop.Writable = true;
                    props.Add(prop);
                }
            }
            currentType = currentType.BaseType;
        }

        return props;
    }
}

public static class JsonIO
{
    static readonly JsonSerializerSettings settings = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new PrivateFieldsContractResolver(),
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
    };
    public static void Write<T>(string pathPre, string name, T obj)
    {
        //Debug.Log("write"+curEntity);
        string path = pathPre +"/" + name + ".json";
        string pathShort = pathPre;
        if (!Directory.Exists(pathShort))
        {
            Directory.CreateDirectory(pathShort);
        }
        // string str = JsonUtility.ToJson(curEntity, true);
        string str = JsonConvert.SerializeObject(obj, settings);
        File.WriteAllText(path, str);
    }
    [CanBeNull]
    public static T Read<T>(string pathPre, string name)
    {
        string path = pathPre + "/" + name + ".json";
        if (!File.Exists(path))
        {
            Debug.Log("path :" + path + " not exist");
            return default;
        }
        string str = File.ReadAllText(path);
        // return JsonUtility.FromJson<T>(str);
        return JsonConvert.DeserializeObject<T>(str, settings);
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
