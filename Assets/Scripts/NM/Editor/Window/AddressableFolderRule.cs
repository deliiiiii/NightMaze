using UnityEditor;
using UnityEngine;

namespace NM.Editor;

[CreateAssetMenu(fileName = nameof(AddressableFolderRule), menuName = "NM/Addressable Folder Rule")]
public sealed class AddressableFolderRule : ScriptableObject
{
    public bool Enable = true;
    public DefaultAsset? Folder;
    public string Tag = string.Empty;

    public string FolderPath => Folder == null ? string.Empty : AssetDatabase.GetAssetPath(Folder);
}
