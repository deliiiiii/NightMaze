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
        int Prop1 { get; }
        int Prop2 { get; }
        int Prop3 { get; }
        List<ModifyPropInfo> ModifyProp1 { get; }
        List<ModifyPropInfo> ModifyProp2 { get; }
        List<ModifyPropInfo> ModifyProp3 { get; }
        
        UniTask OnSpin(PlaySpin playSpin, CancellationToken ct);
    }
    public abstract class MyItem<TEtt, TSub> : ComBase<TEtt, TSub>, IItem
        where TEtt : EttBase<TEtt>, new()
        where TSub : ComBase<TEtt, TSub>
    {
        public MyItem(TEtt belongEtt) : base(belongEtt) {}
        /// 体魄(Body)
        public int Prop1 => ModifyProp1.Sum(m => m.Value);
        /// 理智(Sans)
        public int Prop2 => ModifyProp2.Sum(m => m.Value);
        /// 智识(Lore)
        public int Prop3 => ModifyProp3.Sum(m => m.Value);
        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyProp1 = [];
        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyProp2 = [];
        [JsonProperty(IsReference = false, ItemIsReference = false)]
        public List<ModifyPropInfo> ModifyProp3 = [];
        
        EttBase IItem.BelongEtt => BelongEtt;
        int IItem.Prop1 => Prop1;
        int IItem.Prop2 => Prop2;
        int IItem.Prop3 => Prop3;
        List<ModifyPropInfo> IItem.ModifyProp1 => ModifyProp1;
        List<ModifyPropInfo> IItem.ModifyProp2 => ModifyProp2;
        List<ModifyPropInfo> IItem.ModifyProp3 => ModifyProp3;
        
        [DebuggerStepThrough]UniTask IItem.OnSpin(PlaySpin playSpin, CancellationToken ct) => OnSpin(playSpin, ct);

        protected virtual UniTask OnSpin(PlaySpin playSpin, CancellationToken ct)
        {
            _playSpin = playSpin;
            SelfAddBaseValue(playSpin);
            playSpin.InsertAfter(
                from itemInPlay in playSpin.BelongNode.GetItemByEtt(BelongEtt).ToIEnumerable()
                from itemDes in itemInPlay.Config.DesList
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
        [JsonIgnore]MyOption<PlaySpin> _playSpin;
        public override string ToString()
        {
            return
                (from p in _playSpin
                    from itemInPlay in p.BelongNode.GetItemByEtt(BelongEtt)
                    select $"{GetType().Name}(Prop1: {Prop1}, Prop2: {Prop2}, Prop3: {Prop3}, " +
                           $"belong {itemInPlay})") 
                | GetType().GetNiceName();
        }
    }
    public class ModifyPropInfo 
    {
        public required IItem Ett;
        public required int Value;
    }
}