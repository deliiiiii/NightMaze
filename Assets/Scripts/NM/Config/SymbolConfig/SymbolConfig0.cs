using UnityEngine;

namespace NM.Config;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "NM/" + nameof(SymbolConfig0))]
public class SymbolConfig0 : SymbolConfig
{
    public override int ID => 0;
    [SerializeField] SymbolConfig? tarConfig;
    [SerializeField] SymbolConfig? createConfig;
    public int TarID => tarConfig?.ID ?? -1;
    public int CreateID => createConfig?.ID ?? -1;
}
