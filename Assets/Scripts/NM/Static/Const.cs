namespace NM;

public static class Const
{
    public static class Res
    {
        public static class Config
        {
            public const string EnumPre = "Assets/Config/Mgr/";
            public const string ItemTag = "0_物体Tag";
            public const string SymbolTag = "1_棋子Tag";
            public const string BuildingTag = "2_建筑Tag";
            public const string ResourceTag = "3_资源Tag";
            public const string EventTag = "4_事件Tag";
            public const string GridTag = "5_地块Tag";
        }
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
            public const string TechTreeName = "TechTree";
            
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