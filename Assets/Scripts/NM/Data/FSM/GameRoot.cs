using System;
using System.Collections.Generic;
using GeneralPreview;

namespace NM.Data;

public partial class GameRoot : CompositeBase<DataRoot, GameRoot>
{
    protected override List<HashSet<Type>> MutexListSet => 
    [
        [typeof(GameTitle), typeof(GamePlaying)]
    ];

    public static readonly GameRoot Root = new();
}