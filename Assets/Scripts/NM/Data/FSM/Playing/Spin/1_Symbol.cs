using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
    public partial class Symbol : MyItem<Symbol, GamePlaying.Symbol>
    {
        public Symbol(PlaySpin spin, GamePlaying.Symbol inPlay) : base(spin, inPlay) { }
        protected override void SelfAddBaseValue()
        {
            base.SelfAddBaseValue();
            var config = InPlay.Config;
            if (config.Prop1 != 0)
            {
                ((IItem)this).ModifyPropList.Add(new ModifyPropInfo
                {
                    Ett = this,
                    PropType = EPropType.Prop1,
                    AddValue = config.Prop1
                });
            }
            if (config.Prop2 != 0)
            {
                ((IItem)this).ModifyPropList.Add(new ModifyPropInfo
                {
                    Ett = this,
                    PropType = EPropType.Prop2,
                    AddValue = config.Prop2
                });
            }
            if (config.Prop3 != 0)
            {
                ((IItem)this).ModifyPropList.Add(new ModifyPropInfo
                {
                    Ett = this,
                    PropType = EPropType.Prop3,
                    AddValue = config.Prop3
                });
            }
        }
    }
}