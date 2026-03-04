using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data.Len;

public class LenOption : SerializedMonoBehaviour
{
    public MyOption<int> Len = None;
    public MyOption<int> Len2 = 42;
}