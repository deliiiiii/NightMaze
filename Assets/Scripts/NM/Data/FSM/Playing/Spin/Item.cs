using System.Collections.Generic;
using GeneralPreview;
using Newtonsoft.Json;

namespace NM.Data;

public partial class PlaySpin
{
    public interface IItem
    {
        int Prop1 { get; }
        int Prop2 { get; }
        int Prop3 { get; }
        List<ModifyPropInfo> ModifyProp1 { get; }
        List<ModifyPropInfo> ModifyProp2 { get; }
        List<ModifyPropInfo> ModifyProp3 { get; }
        EttBase BelongEtt { get; }
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
        
        int IItem.Prop1 => Prop1;
        int IItem.Prop2 => Prop2;
        int IItem.Prop3 => Prop3;
        List<ModifyPropInfo> IItem.ModifyProp1 => ModifyProp1;
        List<ModifyPropInfo> IItem.ModifyProp2 => ModifyProp2;
        List<ModifyPropInfo> IItem.ModifyProp3 => ModifyProp3;
        EttBase IItem.BelongEtt => BelongEtt;

        // public void SelfAddBaseValue(PlaySpin playSpin)
        // {
        //     playSpin.BelongNode[BelongEtt].MatchA(some =>
        //     {
        //         var config = some.Config;
        //         ModifyProp1.Add(new ModifyPropInfo
        //         {
        //             Ett = BelongEtt,
        //             Value = config.Prop1
        //         });
        //         ModifyProp2.Add(new ModifyPropInfo
        //         {
        //             Ett = BelongEtt,
        //             Value = config.Prop2
        //         });
        //         ModifyProp3.Add(new ModifyPropInfo
        //         {
        //             Ett = BelongEtt,
        //             Value = config.Prop3
        //         });
        //     });
        // }
    }
    public class ModifyPropInfo 
    {
        public required IItem Ett;
        public required int Value;
    }
}