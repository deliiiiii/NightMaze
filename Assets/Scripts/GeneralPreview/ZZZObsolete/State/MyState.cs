// using System;
//
// internal class MyState : IStateForFSM, IStateForView, IStateForData
// {
//     internal event Action<float>? OnUpdateEvt;
//     internal event Action? OnEnterEvt;
//     internal event Action? OnEnterAfterEvt;
//     internal event Action? OnExitBeforeEvt;
//     internal event Action? OnExitEvt;   
//     
//     #region IStateForFSM
//     public void Enter()
//     {
//         OnEnterEvt?.Invoke();
//         OnEnterAfterEvt?.Invoke();
//     }
//     public void Exit()
//     {
//         OnExitBeforeEvt?.Invoke();
//         OnExitEvt?.Invoke();
//     }
//     public void Update(float dt)
//     {
//         OnUpdateEvt?.Invoke(dt);
//     }
//     #endregion
//     
//     
//     #region IStateForView
//     public IStateForView OnEnterAfter(Action act)
//     {
//         OnEnterAfterEvt += act;
//         return this;
//     }
//     public IStateForView OnExitBefore(Action act)
//     {
//         OnExitBeforeEvt += act;
//         return this;
//     }
//     #endregion
//     
//     
//     #region IStateForData
//     public IStateForData OnEnter(Action act)
//     {
//         OnEnterEvt += act;
//         return this;
//     }
//     public IStateForData OnExit(Action act)
//     {
//         OnExitEvt += act;
//         return this;
//     }
//
//     public IStateForData OnUpdate(Action<float> act)
//     {
//         OnUpdateEvt += act;
//         return this;
//     }
//     #endregion
// }