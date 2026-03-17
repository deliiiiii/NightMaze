// ReSharper disable RedundantUsingDirective
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

[FacIns(typeof(SymbolConfigB1))]
[FacFallback(typeof(SymbolConfig))]
public class SymbolConfigDesB1 : SymbolData.ConfigDesBase<SymbolConfigB1, SymbolConfigDesB1>;
