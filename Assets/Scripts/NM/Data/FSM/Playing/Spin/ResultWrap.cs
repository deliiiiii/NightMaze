using System.Collections.Generic;
using System.Text;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;

public record ResultWrap(ItemDesResultBase? Result, ResultWrap? PreResult, bool DoIfPreFail = false)
{
    public readonly ItemDesResultBase? Result = Result;
    public bool Success;
    [JsonIgnore] bool hasNext;
    public readonly ResultWrap? PreResult = PreResult;
    public readonly bool DoIfPreFail = DoIfPreFail;
    public readonly List<ResultItemWrap> ItemWraps = [];
    public readonly List<ResultPosWrap> PosWraps = [];
    
    protected virtual bool PrintMembers(StringBuilder sb)
    {
        sb.Append($"Result = {Result?.ToString() ?? string.Empty}, ");
        if (PreResult != null)
        {
            PreResult.hasNext = true;
            var preSb = new StringBuilder();
            PreResult.PrintMembers(preSb);
            sb.Append($"PreResult = {{ {preSb} }}, ");
            sb.Append($"DoIfPreFail = {DoIfPreFail}");
        }
        if (hasNext)
        {
            sb.Append($", Success = {Success}, ");
            sb.Append($"ItemWraps = [{string.Join(", ", ItemWraps.Select(w => w))}], ");
            sb.Append($"PosWraps = [{string.Join(", ", PosWraps.Select(w => w))}]");
        }
        return true;
    }
}
public record ResultItemWrap(GamePlaying.MyItem Item)
{
    public GamePlaying.MyItem Item = Item;
    public List<CtxBase> CtxList = [];
    public abstract record CtxBase;
    
    public record CtxSpawned : CtxBase;
    public record CtxRemoved : CtxBase;
    public record CtxSuccessMoved : CtxBase
    {
        public Vector2Int OldPos;
    }
    public record CtxFailMoved : CtxBase;
    public record CtxAddPropX : CtxBase
    {
        public EPropType PropType;
        public long Value;
    }
    public record CtxMulPropX : CtxBase
    {
        public EPropType PropType;
        public double Value;
    }
    protected virtual bool PrintMembers(StringBuilder sb)
    {
        sb.Append($"Item = {Item}, ");
        sb.Append($"CtxList = [{string.Join(", ", CtxList.Select(c => c.GetType()))}]");
        return true;
    }
}
public record ResultPosWrap(Vector2Int Pos)
{
    public Vector2Int Pos = Pos;
    public readonly List<CtxBase> CtxList = [];
    public abstract record CtxBase;
    public record CtxFalse : CtxBase;
    protected virtual bool PrintMembers(StringBuilder sb)
    {
        sb.Append($"Pos = {Pos}, ");
        sb.Append($"CtxList = [{string.Join(", ", CtxList.Select(c => c.GetType()))}]");
        return true;
    }
}