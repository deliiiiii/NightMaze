using Cysharp.Threading.Tasks;

namespace MemoFramework.UI
{
    /// <summary>
    /// 用UniTask实现的UI动画，比协程更为方便，且同样运行在PlayerLoop主线程
    /// </summary>
    public abstract class AsyncUIAnimation : UIAnimation
    {
        public override void Play(ViewBase view)
        {
            base.Play(view);
            DoAnimation(view).ContinueWith(OnEnd).Forget();
        }

        protected abstract UniTask DoAnimation(ViewBase view);
    }
}