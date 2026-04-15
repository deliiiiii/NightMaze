namespace NM;

public static class Const
{
    public static class Res
    {
        // 字段名必须以Tag结尾!
        public static class AddrTag
        {
            public const string ConfigTag = "NMConfig";
            public const string ItemSpriteTag = "NMItemSprite";
        }
        public static class TMP
        {
        }
    }
    public static class Name
    {
        public const string Proj = "NM";
        public const string Name1 = "体魄";
        public const string Name2 = "理智";
        public const string Name3 = "智识";
        public const string NameA1 = "忠诚度";
        public const string NameA2 = "敌意值";
        
        public static class Save
        {
            public const string SlotFolder = "Slot";
            public const string SettingFolder = "Setting";
            public const string SettingName = "Setting";
        }
    }

    public static class World
    {
        public const int GridSize = 1;
    }
    
    public static class SortingLayer
    {
        public const string GridBack = "Grid Back";
        public const string GridBuilding = "Grid Building";
        public const string GridEvent = "Grid Building";
        public const string GridSymbol = "Grid Symbol";
        public const string GridResource = "Grid Resource";
    }
    public static class Layer
    {
        public const string TechUI = "TechUI";
        public const string TechUIHandle = "TechUIHandle";
    }
}