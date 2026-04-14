using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
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

        public void DistributeProp()
        {
            var tarBuildingOrEvt =
                (from pos in inPlay.CoveredPosList
                    from tarItem in spin.BelongNode.Items
                    where tarItem.Config.IsBuildingOrEvent && tarItem.CoveredPosList.Contains(pos)
                    select tarItem).ToList();
            EPropType.GetValues().ForEach(propType =>
            {
                var remain = GetAllProp(propType);
                while (remain > 0 && tarBuildingOrEvt.Any())
                {
                    var tarItem = tarBuildingOrEvt.First();
                    var inProgress = tarItem.BuildingOrEventProgress.GetValueOrDefault(propType, 0);
                    var tarProgress = tarItem.Config.BuildPropValueList.GetValueOrDefault(propType, 0);
                    var require = Math.Max(tarProgress - inProgress, 0);
                    var use = Math.Min(remain, require);
                    if (use > 0)
                    {
                        DistributePropList.Add(new DistributePropInfo
                        {
                            PropType = propType,
                            Value = use,
                            ToItem = tarItem,
                        });
                    }
                    remain -= use;
                    tarBuildingOrEvt.RemoveAt(0);
                }
                if (remain != 0)
                {
                    DistributePropList.Add(new DistributePropInfo
                    {
                        PropType = propType,
                        Value = remain,
                    });
                }
            });
         
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