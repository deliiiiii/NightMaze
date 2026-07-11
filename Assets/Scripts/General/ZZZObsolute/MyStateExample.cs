// using System;
// using UnityEngine;
//
// public class MyStateExample : MyStateBase
// {
//     protected override void OnEnter()
//     {
//         MyDebug.Log("MyStateExample OnEnter", ELogType.State);
//     }
//
//     protected override void OnExit()
//     {
//         MyDebug.Log("MyStateExample OnExit", ELogType.State);
//     }
//
//     protected override void OnUpdate()
//     {
//         if (Input.GetKeyDown(KeyCode.Alpha2))
//         {
//             // GameManager.gameFSM.ChangeState(typeof(MyStateExample2));
//         }
//     }
// }
//
// public class MyStateExample2 : MyStateBase
// {
//     protected override void OnEnter()
//     {
//         MyDebug.Log("MyStateExample2 OnEnter", ELogType.State);
//     }
//
//     protected override void OnExit()
//     {
//         MyDebug.Log("MyStateExample2 OnExit", ELogType.State);
//     }
//
//     protected override void OnUpdate()
//     {
//         if (Input.GetKeyDown(KeyCode.Alpha1))
//         {
//             // GameManager.gameFSM.ChangeState(typeof(MyStateExample));
//         }
//         if (Input.GetKeyDown(KeyCode.Alpha3))
//         {
//             // GameManager.gameFSM.ChangeState(typeof(MyStateExample3));
//         }
//     }
// }
//
// public class MyStateExample3 : MyStateBase
// {
//     protected override void OnEnter()
//     {
//         MyDebug.Log("MyStateExample3 OnEnter", ELogType.State);
//     }
//
//     protected override void OnExit()
//     {
//         MyDebug.Log("MyStateExample3 OnExit", ELogType.State);
//     }
//
//     protected override void OnUpdate()
//     {
//         if (Input.GetKeyDown(KeyCode.Alpha2))
//         {
//             // GameManager.gameFSM.ChangeState(typeof(MyStateExample2));
//         }
//     }
// }