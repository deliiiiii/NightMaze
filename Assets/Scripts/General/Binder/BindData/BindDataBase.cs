using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace General
{
    [Serializable]
    public abstract class BindDataBase : IDisposable
    {
        public void Bind(CancellationToken? ct = null)
        {
            if (guardSet.Any(guard => !guard.Invoke()))
            {
                return;
            }
            if (ct != null)
            {
                this.AddTo(ct.Value);
            }
            BindInternal();
        }
        protected abstract void BindInternal();
        public abstract void UnBind(); 
        readonly HashSet<Func<bool>> guardSet = new ();
        public T Where<T>(Func<bool> guard)
            where T : BindDataBase
        {
            guardSet.Add(guard);
            return this as T;
        }

        public void Dispose() => UnBind();
    }
}


