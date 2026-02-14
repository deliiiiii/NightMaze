using System.Threading;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;
public delegate UniTask UniAction(CancellationToken ct);
public delegate UniTask UniFunc<in T1>(T1 arg1, CancellationToken ct);
public delegate UniTask UniFunc<in T1, in T2>(T1 arg1, T2 arg2, CancellationToken ct);