using General;
using Newtonsoft.Json;

namespace GeneralPreview;

public record struct KeyCompactKvp<TKey, TValue>(TKey Key, TValue Value)
{
    [JsonConverter(typeof(CompactFormatRefConverter))]
    public readonly TKey Key = Key;
    public readonly TValue Value = Value;
    public override string ToString() => $"({Key}:{Value})";
}