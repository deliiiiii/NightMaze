using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralPreview;

public static class EnumExt
{
    extension<T>(T self) where T : Enum
    {
        public static IEnumerable<T> GetValues() => Enum.GetValues(typeof(T)).Cast<T>();

        public List<T> ToValues()
        {
            // 将Flag了的enum值转换为IEnumerable<T>
            return GetValues<T>().Where(flag => self.HasFlag(flag)).ToList();
        }
    }
}