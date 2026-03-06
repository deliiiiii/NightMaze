using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace GeneralPreview;

// public class MyOptionJsonConverter<T> : JsonConverter<MyOption<T>> where T : struct
// {
//     public override void WriteJson(JsonWriter writer, MyOption<T>? value, JsonSerializer serializer)
//     {
//         value!.MatchA(some => serializer.Serialize(writer, some), none: writer.WriteNull);
//     }
//
//     public override MyOption<T> ReadJson(JsonReader reader, Type objectType, MyOption<T>? existingValue, bool hasExistingValue,
//         JsonSerializer serializer)
//     {
//         var token = JToken.Load(reader);
//         if (token.Type == JTokenType.Null)
//         {
//             return MyOption<T>.None;
//         }
//         var value = token.ToObject<T>(serializer);
//         return new MySome<T>(value);
//     }
// }


public class MyOptionJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && 
               objectType.GetGenericTypeDefinition() == typeof(MyOption<>);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        Type type = value.GetType();
        PropertyInfo? valueProp = type.GetProperty("Value");
        if (valueProp != null)
        {
            object? innerValue = valueProp.GetValue(value);
            serializer.Serialize(writer, innerValue);
        }
        else
            writer.WriteNull();
    }
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        Type t = objectType.GetGenericArguments()[0];
        Type someType = typeof(MySome<>).MakeGenericType(t);
        Type noneType = typeof(MyNone<>).MakeGenericType(t);
        if (reader.TokenType == JsonToken.Null)
            return Activator.CreateInstance(noneType);
        object? innerValue = serializer.Deserialize(reader, t);
        return innerValue != null ? Activator.CreateInstance(someType, innerValue) : Activator.CreateInstance(noneType);
    }
}