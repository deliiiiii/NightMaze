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
    public class MyItem(PlaySpin spin, GamePlaying.MyItem inPlay)
    {
        public PlaySpin Spin = spin;
        public GamePlaying.MyItem InPlay {[DebuggerStepThrough] get; init; } = inPlay;

        public long GetProp(EPropType propType)
        {
            var filteredList = ModifyPropList.Where(m => m.PropType == propType).ToList();
            var addSum = filteredList.Sum(m => m.AddValue);
            var multiSum = filteredList.Aggregate(1L, (cur, m) => cur * m.AddValue);
            return addSum * multiSum;
        }

        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyPropList { [DebuggerStepThrough] get; init; } = [];
        public UniTask OnSpin(CancellationToken ct)
        {
            SelfAddBaseValue();
            Spin.InsertAfter(
                from itemDes in (List<ItemDesConfig>)[..InPlay.Config.DesList, ..InPlay.EatConfigList]
                where itemDes.Result != null && itemDes.Trigger is ItemDesTriggerEnterSpin
                select new ActDoItemDesResult(Spin)
                {
                    SelfItem = this,
                    ResultWrap = new ResultWrap(itemDes.Result!, null),
                });
            return UniTask.CompletedTask;
        }

        void SelfAddBaseValue()
        {
            var config = InPlay.Config;
            if (config.IsSymbol)
            {
                config.SymbolPropValueList.ForEach(pair =>
                {
                    ModifyPropList.Add(new ModifyPropInfo
                    {
                        From = this,
                        PropType = pair.Key,
                        AddValue = pair.Value,
                    });
                });
            }
        }
        
        protected virtual bool PrintMembers(StringBuilder sb)
        {
            sb.Append($"{InPlay.PrintMembers()}, ");
            sb.Append($"ModifyPropList = [{string.Join(", ", ModifyPropList)}]");
            return true;
        }
    }
    public record ModifyPropInfo
    {
        public required MyItem From;
        public required EPropType PropType;
        public long AddValue;
        public double MultiValue = 1;

        public bool HasValue => AddValue != 0 || Math.Abs(MultiValue - 1) > 1e-5;

        protected virtual bool PrintMembers(StringBuilder sb)
        {
            sb.Append($"Ett = {From.InPlay.Config.Name},");
            sb.Append($"PropType = {PropType.GetLabelText()}, ");
            sb.Append($"AddValue = {AddValue}, ");
            sb.Append($"MultiValue = {MultiValue}");
            return true; 
        }
    }
}