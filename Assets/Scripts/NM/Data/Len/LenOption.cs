using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;

public class LenOption : SerializedMonoBehaviour
{
    public MyOption<int> Len = None;
    public MyOption<int> Len2 = 42;
}