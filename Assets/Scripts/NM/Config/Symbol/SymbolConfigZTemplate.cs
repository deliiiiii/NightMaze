using System;

namespace NM.Config;

// [UnityEngine.CreateAssetMenu(fileName = "NewSymbol", menuName = "NM/" + nameof(SymbolConfigZTemplate))]
public class SymbolConfigZTemplate : SymbolConfig
{
    void OnEnable()
    {
        ID = -1;
    }
}