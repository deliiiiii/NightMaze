using System;
using System.Collections;

namespace MemoFramework.UI
{
    /// <summary>
    /// 用协程实现的UI动画，需要
    /// </summary>
    public abstract class CoroutineUIAnimation : UIAnimation
    {
        public override void Play(ViewBase view)
        {
            base.Play(view);
            MemoFrameworkEntry.GetComponent<UIComponent>().CoroutineAgent.StartCoroutine(DoAnimation(view,OnEnd));
        }

        protected abstract IEnumerator DoAnimation(ViewBase view,Action onEnd);
    }
}