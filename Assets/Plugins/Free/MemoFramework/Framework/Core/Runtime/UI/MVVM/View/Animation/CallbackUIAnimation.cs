using System;

namespace MemoFramework.UI
{
    /// <summary>
    /// 如果UI动画是通过Dotween或者其他外部方法来实现的，可以继承这个类
    /// 在DoAnimation调用DoTween的方法
    /// 在结束回调中调用OnEnd
    /// </summary>
    public abstract class CallbackUIAnimation : UIAnimation
    {
        public override void Play(ViewBase view)
        {
            base.Play(view);
            DoAnimation(view,OnEnd);
        }

        /// <summary>
        /// 具体的动画实现，需要手动在动画结束时调用OnEnd
        /// </summary>
        /// <param name="onEnd"></param>
        /// 
        protected abstract void DoAnimation(ViewBase view,Action onEnd);
    }
}