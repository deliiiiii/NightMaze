using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public partial class GameRoot : DataBase<GameRoot>
{
    protected override List<HashSet<Type>> MutexListSet => 
    [
        [typeof(GameTitle), typeof(GamePlaying)]
    ];

    public static readonly GameRoot Root = new();
}