namespace NM.View;

public interface ITechObj
{
    void OnCreate();
    void OnStartEdit();
    void OnEndEdit();
    void OnSelect();
    void OnDeSelect();
}