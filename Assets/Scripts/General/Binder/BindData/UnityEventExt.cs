using UnityEngine.Events;

namespace General
{
    public static class UnityEventExt
    {
        public static BindDataEvent EvtBindTo(this UnityEvent self, UnityAction act) 
            => new BindDataEvent(self).To(act);
        public static BindDataEvent<T> EvtBindTo<T>(this UnityEvent<T> self, UnityAction<T> act) 
            => new BindDataEvent<T>(self).To(act);
    }
}