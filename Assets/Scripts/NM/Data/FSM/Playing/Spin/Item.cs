using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
    public class MyItem
    {
        [JsonConstructor]
        public MyItem(){}
        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyPropList { [DebuggerStepThrough] get; init; } = [];
        public List<DistributePropInfo> DistributePropList { [DebuggerStepThrough] get; init; } = [];
        public long GetAllProp(EPropType propType)
        {
            var filteredList = ModifyPropList.Where(m => m.PropType == propType).ToList();
            var addSum = filteredList.Sum(m => m.AddValue);
            var multiSum = filteredList.Aggregate((double)1, (cur, m) => cur * m.MultiValue);
            return (long)(addSum * multiSum);
        }
        public long GetToPlayerProp(EPropType propType)
        {
            if(!DistributePropList.Any())
                return GetAllProp(propType);
            return DistributePropList
                .Where(d => d.ToItem == null && d.PropType == propType)
                .Sum(d => d.Value);
        }
    }
}