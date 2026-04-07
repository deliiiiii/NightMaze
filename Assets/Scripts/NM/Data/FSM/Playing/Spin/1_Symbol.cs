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
    }
}