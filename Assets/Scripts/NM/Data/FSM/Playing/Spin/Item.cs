using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    public abstract class MyItem<TSub, TSubInPlay> : IItem
        where TSub : MyItem<TSub, TSubInPlay>
        where TSubInPlay : GamePlaying.IItem
    {
        protected MyItem(PlaySpin spin, TSubInPlay inPlay)
        {
            Spin = spin;
            InPlay = inPlay;
        }
        protected PlaySpin Spin;
        public TSubInPlay InPlay {[DebuggerStepThrough] get;}
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
                where itemDes.Trigger is ItemDesTriggerEnterSpin
                select new ActDoItemDesResult(Spin)
                {
                    SelfItem = this,
                    Result = itemDes.Result,
                });
            return UniTask.CompletedTask;
        }
        protected virtual void SelfAddBaseValue() { }
        [DebuggerStepThrough] public override string ToString() 
            => $"{GetType().Name} belong {InPlay})";
    }
    public class ModifyPropInfo 
    {
        public required IItem Ett;
        public required EPropType PropType;
        public long AddValue;
        public long MultiValue;
    }
}