using GeneralPreview;

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
                ModifyProp1.Add(new ModifyPropInfo
                {
                    Ett = this,
                    Value = config.Prop1
                });
                ModifyProp2.Add(new ModifyPropInfo
                {
                    Ett = this,
                    Value = config.Prop2
                });
                ModifyProp3.Add(new ModifyPropInfo
                {
                    Ett = this,
                    Value = config.Prop3
                });
            });
        }
    }
}