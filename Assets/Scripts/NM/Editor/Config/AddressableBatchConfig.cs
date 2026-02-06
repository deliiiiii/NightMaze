using System;
using System.Collections.Generic;
using GeneralProj;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Editor
{
    [Serializable]
    internal class BatchRule
    {
        public bool Enable = true;
        public string FolderPath = "Assets/";
        public string TagName;
    }

    internal class AddressableBatchConfig : ScriptableObject
    { 
        [Button("Open Batch Window", ButtonSizes.Large), PropertyOrder(-1)]
        void OpenWindow()
        {
            AddressableBatchProcessor.ShowWindowWithArg(this);
        }

        [ReadOnly] public List<BatchRule> RuleList = new();
    }
}