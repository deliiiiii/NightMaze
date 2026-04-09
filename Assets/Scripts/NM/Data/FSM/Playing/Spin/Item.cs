using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
    public interface IItem
    {
        GamePlaying.IItem InPlay { get; }
        long GetProp(EPropType propType);
        List<ModifyPropInfo> ModifyPropList { get; }
        
        UniTask OnSpin(CancellationToken ct);
    }
    public abstract record MyItem<TSub, TSubInPlay> : IItem
        where TSub : MyItem<TSub, TSubInPlay>
        where TSubInPlay : GamePlaying.IItem
    {
        protected MyItem(PlaySpin spin, TSubInPlay inPlay)
        {
            Spin = spin;
            InPlay = inPlay;
        }
        protected PlaySpin Spin;
        protected TSubInPlay InPlay {[DebuggerStepThrough] get; init; }
        GamePlaying.IItem IItem.InPlay { [DebuggerStepThrough] get => InPlay; }
        public long GetProp(EPropType propType)
        {
            var filteredList = ((IItem)this).ModifyPropList.Where(m => m.PropType == propType).ToList();
            var addSum = filteredList.Sum(m => m.AddValue);
            var multiSum = filteredList.Aggregate(1L, (cur, m) => cur * m.AddValue);
            return addSum * multiSum;
        }
        [JsonProperty(IsReference = false, ItemIsReference = false)] 
        public List<ModifyPropInfo> ModifyPropList { [DebuggerStepThrough] get; } = [];
        public virtual UniTask OnSpin(CancellationToken ct)
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
        protected virtual void SelfAddBaseValue() { }
        
        protected virtual bool PrintMembers(StringBuilder sb)
        {
            sb.Append($"{InPlay.PrintMembers()}, ");
            sb.Append($"ModifyPropList = [{string.Join(", ", ModifyPropList)}]");
            return true;
        }
    }
    public record ModifyPropInfo 
    {
        public required IItem From;
        public required EPropType PropType;
        public long AddValue;
        public long MultiValue = 1;
        public bool HasValue => AddValue != 0 || MultiValue != 1;

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