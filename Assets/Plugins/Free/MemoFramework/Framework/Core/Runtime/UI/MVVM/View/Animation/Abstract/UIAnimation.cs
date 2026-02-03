using System;

namespace MemoFramework.UI
{
    public abstract class UIAnimation
    {
        private Action _onStart;
        private Action _onEnd;

        protected void OnStart()
        {
            try
            {
                if (this._onStart != null)
                {
                    this._onStart();
                    this._onStart = null;
                }
            }
            catch (Exception e)
            {
                MFLogger.LogError("UI动画OnStart时出现异常：" + e.Message);
            }
        }

        protected void OnEnd()
        {
            try
            {
                if (this._onEnd != null)
                {
                    this._onEnd();
                    this._onEnd = null;
                }
            }
            catch (Exception e)
            {
                MFLogger.LogError("UI动画OnEnd时出现异常：" + e.Message);
            }
        }

        public UIAnimation OnStart(Action onStart)
        {
            this._onStart += onStart;
            return this;
        }

        public UIAnimation OnEnd(Action onEnd)
        {
            this._onEnd += onEnd;
            return this;
        }

        public virtual void Play(ViewBase view)
        {
            OnStart();
        }
    }
}