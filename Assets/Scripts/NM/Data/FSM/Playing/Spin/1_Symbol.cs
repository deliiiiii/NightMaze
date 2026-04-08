using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
    public MyOption<Symbol> this[EttSymbol ettId] => GetEttComOptional<EttSymbol, Symbol>(ettId);
    public partial class Symbol : MyItem<EttSymbol, Symbol>
    {
        public Symbol(EttSymbol belongEtt) : base(belongEtt)
        {
        }

        protected override void SelfAddBaseValue(PlaySpin playSpin)
        {
            base.SelfAddBaseValue(playSpin);
            playSpin.BelongNode[BelongEtt].MatchA(some =>
            {
                var config = some.Config;
                if (config.Prop1 != 0)
                {
                    ModifyPropList.Add(new ModifyPropInfo
                    {
                        Ett = this,
                        PropType = EPropType.Prop1,
                        AddValue = config.Prop1
                    });
                }
                if (config.Prop2 != 0)
                {
                    ModifyPropList.Add(new ModifyPropInfo
                    {
                        Ett = this,
                        PropType = EPropType.Prop2,
                        AddValue = config.Prop2
                    });
                }
                if (config.Prop3 != 0)
                {
                    ModifyPropList.Add(new ModifyPropInfo
                    {
                        Ett = this,
                        PropType = EPropType.Prop3,
                        AddValue = config.Prop3
                    });
                }
            });
        }
    }
}