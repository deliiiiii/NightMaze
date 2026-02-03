using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class Updater : Singleton<Updater>
{
    readonly SortedDictionary<int, HashSet<BindDataUpdate>> updateDic = new();
    public static SortedDictionary<int, HashSet<BindDataUpdate>> UpdateDic => Instance.updateDic;
    void Update()
    {
        UpdateDic.Values.ForEach(set =>
        {
            set.ForEach(v =>
            {
                // if(v.GuardSet.All(guard => guard()))
                    v.Act(Time.deltaTime);
            });
        });
    }
}
