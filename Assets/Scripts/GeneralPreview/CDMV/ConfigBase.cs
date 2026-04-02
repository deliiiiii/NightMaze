using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace GeneralPreview;

[Serializable]
    public abstract class ConfigBase : SerializedScriptableObject, IDisposable
    {
        public abstract void OnLoad();
        public abstract void OnUnload();

        void IDisposable.Dispose() => OnUnload();
    }

    public abstract class ConfigSingle<T> : ConfigBase, IRefSingle
        where T : ConfigSingle<T>
    {
        public sealed override void OnLoad() => RefPoolSingle<T>.Register(() => (T)this);
        public sealed override void OnUnload() => RefPoolSingle<T>.Release();
    }

    [Serializable]
    public abstract class ConfigMulti<T>: ConfigBase, IRefMulti
        where T : ConfigMulti<T>
    {
        [OnValueChanged(nameof(OnNameAndIdChanged))] 
        [ValidateInput(nameof(CheckName), "名称不能为空")]
        public string Name = string.Empty;

        public sealed override void OnLoad() => RefPoolMulti<T>.RegisterOne(() => (T)this);
        public sealed override void OnUnload() => RefPoolMulti<T>.ReleaseOne((T)this);

        [OnValueChanged(nameof(OnNameAndIdChanged))]
        [ValidateInput(nameof(CheckNameAndIdIdentical), "名称为空，或ID在当前文件夹(配置类相同)有重复")]
        public int ID;
        
        protected abstract string PrefixName { get; }

        bool CheckAll() => CheckName() && CheckNameAndIdIdentical();
        // bool CheckId() => true;
        bool CheckName() => Name != string.Empty;
        bool CheckNameAndIdIdentical()
        {
    #if UNITY_EDITOR
            if (!CheckName())
                return false;
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
        [JsonIgnore]string NewName => $"{PrefixName}_{ID}_{Name}.asset";
        
        void OnNameAndIdChanged()
        {
    #if UNITY_EDITOR
            if (!CheckAll())
                return;
            UnityEditor.AssetDatabase.RenameAsset(UnityEditor.AssetDatabase.GetAssetPath(this), NewName);
    #endif
        }
    }