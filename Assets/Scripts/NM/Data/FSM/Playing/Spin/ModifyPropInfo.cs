using System;
using System.Text;
using General;
using NM.Config;

namespace NM.Data;

public class ModifyPropInfo
{
    public required GamePlaying.MyItem From;
    public required EPropType PropType;
    public long AddValue;
    public double MultiValue = 1;

    public bool HasValue => AddValue != 0 || Math.Abs(MultiValue - 1) > 1e-5;
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Ett = {From.Config.Name},");
        sb.Append($"PropType = {PropType.GetLabelText()}, ");
        sb.Append($"AddValue = {AddValue}, ");
        sb.Append($"MultiValue = {MultiValue}");
        return sb.ToString();
    }
}