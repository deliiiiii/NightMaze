// using System;
// using System.Collections.Generic;
// using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif
//
// public abstract partial class ModelBase : MonoBehaviour
// {
//     public abstract void Init();
//     public abstract void Launch();
//
//     public static readonly Dictionary<Type, ModelBase> ModelDic = new();
// }
//
//
// #if UNITY_EDITOR
// public partial class ModelBase
// {
//     void Awake()
//     {
//         EditorApplication.playModeStateChanged -= OnExitPlayMode;
//         EditorApplication.playModeStateChanged += OnExitPlayMode;
//     }
//
//     void OnExitPlayMode(PlayModeStateChange state)
//     {
//         if(state == PlayModeStateChange.ExitingPlayMode)
//         {
//             ModelDic.Clear();
//         }
//     }
// }
// #endif