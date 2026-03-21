using System;
using System.Reflection;
using Sirenix.OdinInspector;

namespace GeneralPreview;

public static class EnumExt
{
    extension(Enum? value)
    {
        public string GetLabelText()
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