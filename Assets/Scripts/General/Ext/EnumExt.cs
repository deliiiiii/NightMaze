using System;
using System.Reflection;
using Sirenix.OdinInspector;

namespace General
{
    public static class EnumExt
    {
        public static string GetLabelText(this Enum value)
        {
            if (value == null) 
                return string.Empty;
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) 
                return value.ToString();
            var attribute = fieldInfo.GetCustomAttribute<LabelTextAttribute>();
            return attribute != null ? attribute.Text : value.ToString();
        }
    }
}