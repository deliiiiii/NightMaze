using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin
{
    public interface IItem
    {
        EttBase BelongEtt { get; }
        long GetProp(EPropType propType);
        List<ModifyPropInfo> ModifyPropList { get; }
        
        UniTask OnSpin(PlaySpin playSpin, CancellationToken ct);
    }
    public abstract class MyItem<TEtt, TSub> : ComBase<TEtt, TSub>, IItem
        where TEtt : EttBase<TEtt>, new()
        where TSub : ComBase<TEtt, TSub>
    {
        public MyItem(TEtt belongEtt) : base(belongEtt) {}
        
        
        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyPropList = [];
        
        EttBase IItem.BelongEtt => BelongEtt;
        long IItem.GetProp(EPropType propType)
        {
            var filteredList = ModifyPropList.Where(m => m.PropType == propType).ToList();
            var addSum = filteredList.Sum(m => m.AddValue);
            var multiSum = filteredList.Aggregate(1L, (cur, m) => cur * m.AddValue);
            return addSum * multiSum;
        }
        List<ModifyPropInfo> IItem.ModifyPropList => ModifyPropList;
        
        [DebuggerStepThrough]UniTask IItem.OnSpin(PlaySpin playSpin, CancellationToken ct) => OnSpin(playSpin, ct);

        protected virtual UniTask OnSpin(PlaySpin playSpin, CancellationToken ct)
        {
            pplaySpin = playSpin;
            SelfAddBaseValue(playSpin);
            playSpin.InsertAfter(
                from itemInPlay in playSpin.BelongNode.GetItemByEtt(BelongEtt).ToIEnumerable()
                from itemDes in (List<ItemDesConfig>)[..itemInPlay.Config.DesList, ..itemInPlay.EatConfigList]
                where itemDes.Trigger is ItemDesTriggerEnterSpin
                select new ActDoItemDesResult(playSpin)
                {
                    SelfItem = this,
                    Result = itemDes.Result,
                });
            return UniTask.CompletedTask;
        }
        protected virtual void SelfAddBaseValue(PlaySpin playSpin)
        {
        }

        [JsonIgnore] MyOption<PlaySpin> pplaySpin = null!;
        [DebuggerStepThrough]
        public override string ToString()
        {
            return
                (from p in pplaySpin
                    from itemInPlay in p.BelongNode.GetItemByEtt(BelongEtt)
                    select $"{GetType().Name} belong {itemInPlay})") 
                | GetType().GetNiceName();
        }
    }
    public class ModifyPropInfo 
    {
        public required IItem Ett;
        public required EPropType PropType;
        public long AddValue;
        public long MultiValue;
    }
}