using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace General
{
    public class Updater : Singleton<Updater>
    {
        readonly SortedDictionary<int, HashSet<BindDataUpdate>> updateDic = new();
        public static SortedDictionary<int, HashSet<BindDataUpdate>> UpdateDic => Instance.updateDic;
        void Update()
        {
            foreach (var set in UpdateDic.Values)
            {
                foreach (var v in set)
                {
                    v.Act(Time.deltaTime);
                }
            }
        }
    }
}
