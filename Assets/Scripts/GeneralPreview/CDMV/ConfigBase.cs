using System;
using System.IO;
using System.Linq;
using General;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace GeneralPreview;

[Serializable]
public abstract class ConfigBase : SerializedScriptableObject
{
    public abstract void OnLoad();
}

public abstract class ConfigSingle : ConfigBase, IRefSingle
{
    public sealed override void OnLoad() => RefPoolSingle<ConfigSingle>.Register(() => this);
}

public abstract class ConfigSingle<T> : ConfigSingle
    where T : ConfigSingle<T>, new();

[Serializable]
public abstract class ConfigMulti: ConfigBase, IRefMulti
{
    [OnValueChanged(nameof(OnNameAndIdChanged))] 
    [ValidateInput(nameof(CheckName), "名称不能为空")]
    public string Name = string.Empty;

    [OnValueChanged(nameof(OnNameAndIdChanged))]
    [ValidateInput(nameof(CheckId), "ID不能为空且不能为负数")]
    [ValidateInput(nameof(CheckNameAndIdIdentical), "ID在当前文件夹(配置类相同)有重复")]
    public int ID;
    
    protected abstract string PrefixName { get; }

    protected virtual bool CheckAll()
    {
        return CheckId() && CheckName() && CheckNameAndIdIdentical();
    }
    
    bool CheckId() => ID >= 0;
    bool CheckName() => Name != string.Empty;
    bool CheckNameAndIdIdentical()
    {
#if UNITY_EDITOR
        // if (!CheckName())
        //     return false;
        var thisPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        var directoryName = Path.GetDirectoryName(thisPath);
        // 获取该目录下所有ScriptableObject
        return Directory.GetFiles(directoryName!, "*.asset")
            .Select(path => path.Replace('\\', '/'))
            .Where(path => path != thisPath && path.Split('_')[0].Split('/')[^1] == PrefixName)
            .All(path => int.Parse(path.Split('_')[1]) != ID);
#endif
#pragma warning disable CS0162 // 检测到不可到达的代码
        // ReSharper disable once HeuristicUnreachableCode
        return true;
#pragma warning restore CS0162 // 检测到不可到达的代码
    }
    string newName => $"{PrefixName}_{ID}_{Name}.asset";
    
    void OnNameAndIdChanged()
    {
#if UNITY_EDITOR
        if (!CheckAll())
            return;
        UnityEditor.AssetDatabase.RenameAsset(UnityEditor.AssetDatabase.GetAssetPath(this), newName);
#endif
    }
}
[Serializable]
public abstract class ConfigMulti<T> : ConfigMulti
    where T : ConfigMulti<T>, new()
{
    public sealed override void OnLoad() => RefPoolMulti<T>.RegisterOne(() => (this as T)!);
}