using GeneralPreview;
using NM.Data;
using UnityEngine;

namespace NM.View;

public class SymbolView : ViewBase
{
    [SerializeReference] public required SymbolEtt SymbolEtt;

    protected override void Bind()
    {
    }
}