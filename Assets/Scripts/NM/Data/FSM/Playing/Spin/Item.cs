using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin
{
    public class MyItem
    {
        public long GetProp(EPropType propType)
        {
            var filteredList = ModifyPropList.Where(m => m.PropType == propType).ToList();
            var addSum = filteredList.Sum(m => m.AddValue);
            var multiSum = filteredList.Aggregate((double)1, (cur, m) => cur * m.MultiValue);
            return (long)(addSum * multiSum);
        }

        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyPropList { [DebuggerStepThrough] get; init; } = [];
        public UniTask OnSpinAsync(PlaySpin spin, GamePlaying.MyItem inPlay, CancellationToken ct)
        {
            SelfAddBaseValue(spin, inPlay);
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

        void SelfAddBaseValue(PlaySpin spin, GamePlaying.MyItem inPlay)
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
    public class ModifyPropInfo
    {
        public required GamePlaying.MyItem From;
        public required EPropType PropType;
        public long AddValue;
        public double MultiValue = 1;

        public bool HasValue => AddValue != 0 || Math.Abs(MultiValue - 1) > 1e-5;

        protected virtual bool PrintMembers(StringBuilder sb)
        {
            sb.Append($"Ett = {From.Config.Name},");
            sb.Append($"PropType = {PropType.GetLabelText()}, ");
            sb.Append($"AddValue = {AddValue}, ");
            sb.Append($"MultiValue = {MultiValue}");
            return true; 
        }
    }
}