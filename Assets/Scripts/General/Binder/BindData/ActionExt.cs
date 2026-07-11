using System;

namespace General
{
    public static class ActionExt
    {
        public static BindDataUpdate ActBindTo(this Action<float> self, EUpdatePri priority = EUpdatePri.Default)
            => new(self, priority);
        public static BindDataUpdate ActBindTo<TEnum>(this Action<float> self, TEnum priority) where TEnum : Enum
            => new(self, (EUpdatePri)Convert.ToInt32(priority));
    }
}