using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace General
{
    public class DoTweenSeqMutex
    {
        [CanBeNull] DOTweenSequence curTween;

        public async UniTask PlayMutexAsync([NotNull] DOTweenSequence sequence, CancellationToken ct)
        {
            if(curTween != null)
                curTween.DOKill();
            curTween = sequence;
            await curTween.PlayAsync(ct);
        }
    }
}