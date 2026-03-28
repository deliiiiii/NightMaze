// using System.Collections.Generic;
// using System.Linq;
// using Cysharp.Threading.Tasks;
// using General;
// using GeneralPreview;
// using Sirenix.OdinInspector;
// using Sirenix.Utilities;
// using UnityEngine;
//
// namespace NM.View.ZZZTest;
//
//
// public class TestECs : MonoBehaviour
// {
//     [Button]
//     public void Test()
//     {
//         var nodeUnit = new NodeUnit();
//         var statePlay = new NodeUnit.StatePlay();
//         nodeUnit.ChangeState(statePlay, false).Forget();
//         // var idle = new NodeUnit.StatePlay.StateIdle();
//         // statePlay.ChangeState(idle, false).Forget();
//     }
// }
//
// public record EttContext : EttBase<EttContext>;
// public record EttCard : EttBase<EttCard>;
//
// public class NodeUnit : Node<NodeUnit>
// {
//     Node? state;
//     public UniTask ChangeState<T>(T com, bool isNewFromLoad) where T : RootStateBase<T>
//         => _ChangeAsync(ref state, com, isNewFromLoad);
//     
//     public abstract class RootStateBase<T> : Node<NodeUnit, T> where T : RootStateBase<T>;
//     public class StateTitle : RootStateBase<StateTitle>;
//     /// <summary>
//     /// comDic:
//     /// [
//     ///     typeof(EttCard) :
//     ///     [
//     ///         1 : Card{configID, ...}
//     ///         2 : Card{configID, ...}
//     ///     ],
//     /// ]
//     /// </summary>
//     public class StatePlay : RootStateBase<StatePlay>
//     {
//         public class Card : EttCard.ICom, INodeCom;
//         public IEnumerable<EttCard> EttCardList => GetEttList<EttCard>();
//         MyOption<Card> this[EttCard ett] => GetEttCom<EttCard, Card>(ett);
//         
//         Node? state;
//         Node? env;
//         public UniTask ChangeState<T>(T com, bool isNewFromLoad) where T : PlayStateBase<T>
//             => _ChangeAsync(ref state, com, isNewFromLoad);
//         public UniTask ChangeEnv<T>(T com, bool isNewFromLoad) where T : EnvBase<T>
//             => _ChangeAsync(ref env, com, isNewFromLoad);
//
//         protected override void OnCreateFreshData()
//         {
//             state = new StateSpin();
//             env = new EnvSunState();
//             Enumerable.Range(0, 5)
//                 .Select(i => new EttCard())
//                 .ForEach(ettCard =>
//                 {
//                     AddEttCom(ettCard, new Card());
//                 });
//         }
//
//         protected override async UniTask OnLaunchCom(bool isThisFromLoad)
//         {
//             await state!.OnCreateAsync(isThisFromLoad);
//             await env!.OnCreateAsync(isThisFromLoad);
//         }
//
//         protected override void OnReleaseCom()
//         {
//             state?.OnRemove();
//             env?.OnRemove();
//         }
//
//         public abstract class PlayStateBase<T> : Node<StatePlay, T> where T : PlayStateBase<T>;
//
//         public class StateIdle : PlayStateBase<StateIdle>
//         {
//             protected override void OnCreateFreshData()
//             {
//                 MyDebug.Log("StateIdle OnCreate");
//             }
//         }
//         public class StateSpin : PlayStateBase<StateSpin>
//         {
//             public class Card : EttCard.ICom, INodeCom;
//             SpinStateBase? state;
//
//             public UniTask ChangeState<T>(T com, bool isNewFromLoad) where T : SpinStateBase
//                 => _ChangeAsync(ref state, com, isNewFromLoad);
//             public abstract class SpinStateBase : Node<SpinStateBase>;
//             public class StateBefore : SpinStateBase;
//             public class StateAfter : SpinStateBase;
//         }
//         
//         public abstract class EnvBase<T> : Node<StatePlay, T> where T : EnvBase<T>;
//         public class EnvSunState : EnvBase<EnvSunState>;
//         public class EnvRainState : EnvBase<EnvRainState>;
//     }
// }