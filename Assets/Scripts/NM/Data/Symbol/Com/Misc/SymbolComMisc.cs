using Sirenix.OdinInspector;

namespace NM.Data;

public abstract class DoCountBase;
public class DoCountInfinite : DoCountBase;
public class DoCountNumber : DoCountBase
{
    [MinValue(1)]public int N = 1;
}
public class SymbolComStock : SymbolData.ComBase
{
    public int Count;
}

public class SymbolComEveryNSpin : SymbolData.ComBase
{
    public int Count;
}