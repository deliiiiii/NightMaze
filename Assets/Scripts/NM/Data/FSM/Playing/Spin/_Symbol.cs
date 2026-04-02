using System;
using System.Collections.Generic;
using GeneralPreview;

namespace NM.Data;

public partial class PlaySpin
{
    public MyOption<Symbol> this[EttSymbol symbol] => GetEttComOptional<EttSymbol, Symbol>(symbol);
    public class Symbol : ComBase<EttSymbol, Symbol>
    {
        public int Prop1 => ModifyProp1.Sum(m => m.Value);
        public int Prop2 => ModifyProp2.Sum(m => m.Value);
        public int Prop3 => ModifyProp3.Sum(m => m.Value);
        public List<ModifyPropInfo> ModifyProp1 = [];
        public List<ModifyPropInfo> ModifyProp2 = [];
        public List<ModifyPropInfo> ModifyProp3 = [];

        public Symbol(PlaySpin thisNode, EttSymbol belongEtt) : base(thisNode, belongEtt)
        {
            SelfAddBaseValue = () =>
            {
                thisNode.BelongNode[belongEtt].MatchA(some =>
                {
                    var config = some.Config;
                    ModifyProp1.Add(new ModifyPropInfo
                    {
                        Ett = belongEtt,
                        Value = config.Prop1
                    });
                    ModifyProp2.Add(new ModifyPropInfo
                    {
                        Ett = belongEtt,
                        Value = config.Prop2
                    });
                    ModifyProp3.Add(new ModifyPropInfo
                    {
                        Ett = belongEtt,
                        Value = config.Prop3
                    });
                });
            };
        }

        public Action SelfAddBaseValue;
    }
    public class ModifyPropInfo 
    {
        public required EttBase Ett;
        public required int Value;
    }
}