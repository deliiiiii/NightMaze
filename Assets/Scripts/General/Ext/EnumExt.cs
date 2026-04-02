using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Sirenix.OdinInspector;

namespace General
{
    public static class EnumExt
    {
        public static string GetLabelText([CanBeNull] this Enum value)
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