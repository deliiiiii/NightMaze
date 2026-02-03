using System;

[Serializable]
public class Buff
{
    public Buff(float value, string desc = "")
    {
        Value = value;
        Desc = desc;
    }
    public readonly float Value;
    public readonly string Desc;
}