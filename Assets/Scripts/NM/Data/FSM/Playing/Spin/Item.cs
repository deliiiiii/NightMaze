using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin
{
    public class MyItem(PlaySpin spin, GamePlaying.MyItem inPlay)
    {
        [JsonConstructor]
        MyItem() : this(null!, null!){}

        PlaySpin spin = spin;
        GamePlaying.MyItem inPlay = inPlay;

        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyPropList { [DebuggerStepThrough] get; init; } = [];

        public long GetProp(EPropType propType)
        {
            var filteredList = ModifyPropList.Where(m => m.PropType == propType).ToList();
            var addSum = filteredList.Sum(m => m.AddValue);
            var multiSum = filteredList.Aggregate((double)1, (cur, m) => cur * m.MultiValue);
            return (long)(addSum * multiSum);
        }
        public UniTask OnSpinAsync()
        {
            SelfAddBaseValue();
            spin.InsertAfter(
                from itemDes in (List<ItemDesConfig>)[..inPlay.Config.DesList, ..inPlay.EatConfigList]
                where itemDes.Result != null && itemDes.Trigger is ItemDesTriggerEnterSpin
                select new ActDoItemDesResult(spin)
                {
                    Item = inPlay,
                    ResultWrap = new ResultWrap(itemDes.Result!, null),
                });
            return UniTask.CompletedTask;
        }

        void SelfAddBaseValue()
        {
            var config = inPlay.Config;
            if (config.IsSymbol)
            {
                config.SymbolPropValueList.ForEach(pair =>
                {
                    ModifyPropList.Add(new ModifyPropInfo
                    {
                        From = inPlay,
                        PropType = pair.Key,
                        AddValue = pair.Value,
                    });
                });
            }
        }
    }
}