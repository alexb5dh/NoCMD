using System.Collections.Generic;

namespace NoCMD.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue @default)
        {
            TValue value;
            var exists = dictionary.TryGetValue(key, out value);
            return exists ? value : @default;
        }
    }
}
