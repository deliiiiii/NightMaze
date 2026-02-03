#nullable enable
using System;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Observable<T> where T : struct
{
    [SerializeField] T value;
    bool invokeEvenEqual;
    bool acceptDuplicateEvent;
    public T Value
    {
        get => value;
        set
        {
            var oldV = this.value;
            if (value is IComparable com)
            {
                if (com.CompareTo(oldV) == 0 && !invokeEvenEqual)
                {
                    return;
                }
            }
            else if (value.Equals(oldV) && !invokeEvenEqual)
                return;
            this.value = value;
            onValueChangedAfter?.Invoke(this.value);
            onValueChangedFull?.Invoke(oldV, value);
        }
    }
    [JsonIgnore] UnityAction<T>? onValueChangedAfter;
    public event UnityAction<T>? OnValueChangedAfter
    {
        add
        {
            if (!acceptDuplicateEvent || (!onValueChangedAfter?.GetInvocationList().Contains(value) ?? true))
            {
                onValueChangedAfter += value;
            }
        }
        remove => onValueChangedAfter -= value;
    }
    [JsonIgnore] UnityAction<T, T>? onValueChangedFull;
    public event UnityAction<T, T>? OnValueChangedFull
    {
        add
        {
            if (!acceptDuplicateEvent || (!onValueChangedFull?.GetInvocationList().Contains(value) ?? true))
            {
                onValueChangedFull += value;
            }
        }
        remove => onValueChangedFull -= value;
    }
    [JsonConstructor]
    public Observable(T initValue)
    {
        value = initValue;
    }
    public Observable(T initValue, 
        UnityAction<T>? before = null, 
        UnityAction<T>? after = null,
        bool invokeEvenEqual = false,
        bool acceptDuplicateEvent = false
        )
    {
        value = initValue;
        onValueChangedAfter += after;
        this.invokeEvenEqual = invokeEvenEqual;
        this.acceptDuplicateEvent = acceptDuplicateEvent;
    }
    public static implicit operator T(Observable<T> v)
    {
        return v.Value;
    }
    
    public static implicit operator float(Observable<T> v)
    {
        return v.Value switch
        {
            int i => i,
            float f => f,
            double d => (float)d,
            char c => c,
            _ => throw new InvalidCastException($"Cannot convert {v.value} (Type: {typeof(T)}) to float")
        };
    }


    public override string ToString()
    {
        return Value.ToString();
    }
    
    public const string NameOfValue = nameof(value); 
}


// [Serializable]
// public class ObservableInt : Observable<int>
// {
//     public ObservableInt(int initValue) : base(initValue)
//     {
//     }
//
//     public ObservableInt(int initValue, [CanBeNull] UnityAction<int> before = null, [CanBeNull] UnityAction<int> after = null, bool forceEverySet = false, bool enableRepeatEvent = false) : base(initValue, before, after, forceEverySet, enableRepeatEvent)
//     {
//     }
// }
//
// [Serializable]
// public class ObservableFloat : Observable<float>
// {
//     public ObservableFloat(float initValue) : base(initValue)
//     {
//     }
//
//     public ObservableFloat(float initValue, [CanBeNull] UnityAction<float> before = null, [CanBeNull] UnityAction<float> after = null, bool forceEverySet = false, bool enableRepeatEvent = false) : base(initValue, before, after, forceEverySet, enableRepeatEvent)
//     {
//     }
// }
//
// [Serializable]
// public class ObservableBool : Observable<bool>
// {
//     public ObservableBool(bool initValue) : base(initValue)
//     {
//     }
//
//     public ObservableBool(bool initValue, [CanBeNull] UnityAction<bool> before = null, [CanBeNull] UnityAction<bool> after = null, bool forceEverySet = false, bool enableRepeatEvent = false) : base(initValue, before, after, forceEverySet, enableRepeatEvent)
//     {
//     }
// }

// [Serializable]
// public class ObservableEnum : Observable<Enum>
// {
//     public ObservableEnum(Enum initValue) : base(initValue)
//     {
//     }
//
//     public ObservableEnum(Enum initValue, [CanBeNull] UnityAction<Enum> before = null, [CanBeNull] UnityAction<Enum> after = null, bool forceEverySet = false, bool enableRepeatEvent = false) : base(initValue, before, after, forceEverySet, enableRepeatEvent)
//     {
//     }
// }