using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using General.BindData;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public abstract class ViewBase : MonoBehaviour
{
    protected virtual IEnumerable<BindDataBase> BindList() => [];

    protected virtual void Awake()
    {
        BindList().ForEach(b => b.Bind());
        GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(propertyInfo =>
            {
                var type = propertyInfo.PropertyType;
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UniEvt<>);
            })
            .ForEach(propertyInfo => ((IDisposable)propertyInfo.GetMemberValue(this)).AddTo(destroyCancellationToken));
    }

    protected virtual void OnDestroy()
    {
        BindList().ForEach(b => b.UnBind());
    }
}