using System;

namespace NM.Config;

[Serializable]
public class BatchRule
{
    public bool Enable = true;
    public string FolderPath = "Assets/";
    public string TagName = string.Empty;
}