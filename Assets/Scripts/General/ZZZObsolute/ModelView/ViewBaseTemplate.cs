// #if UNITY_EDITOR
// using UnityEditor;
//
//
// public partial class ViewBase
// {
//     // protected static MainView MainView;
//     // protected static BattleView BattleView;
//
//     void Awake()
//     {
//         EditorApplication.playModeStateChanged -= OnExitPlayMode;
//         EditorApplication.playModeStateChanged += OnExitPlayMode;
//     }
//
//     static void OnExitPlayMode(PlayModeStateChange state)
//     {
//         if(state == PlayModeStateChange.ExitingPlayMode)
//         {
//             // MainView = null;
//             // BattleView = null;
//         }
//     }
// }
//
// #endif