namespace NM;

public struct Const
{
    public struct Await
    {
        public const int OneFramePerSpawn = 10;
    }
    public struct Res
    {
        public struct Config
        {
            public const string Path = "Assets/Config";
            public const string EnumPre = "Assets/Config/Mgr/";
            public const string ItemTag = "0_物体Tag";
            public const string SymbolTag = "1_棋子Tag";
            public const string BuildingTag = "2_建筑Tag";
            public const string ResourceTag = "3_资源Tag";
            public const string EventTag = "4_事件Tag";
            public const string GridTag = "5_地块Tag";
        }

        public struct Item
        {
            public const string SpritePath = "Assets/Art/Sprite/Item";
        }
        // 字段名必须以Tag结尾!
        public struct AddrTag
        {
            public const string ConfigTag = "NMConfig";
            public const string ItemSpriteTag = "NMItemSprite";
        }
        public struct TMP
        {
        }
    }
    public struct Name
    {
        public const string Proj = "NM";
        public const string Name1 = "体魄";
        public const string Name2 = "理智";
        public const string Name3 = "智识";
        public const string NameA1 = "忠诚度";
        public const string NameA2 = "敌意值";
        
        public struct Save
        {
            public const string SlotFolder = "Slot";
            public const string SettingFolder = "Setting";
            public const string SettingName = "Setting";
            public const string TechTreeName = "TechTree";
            
        }
    }

    public struct World
    {
        public const int GridSize = 1;
    }
    
    public struct SortingLayer
    {
        public const string GridBack = "Grid Back";
        public const string GridBuilding = "Grid Building";
        public const string GridEvent = "Grid Building";
        public const string GridSymbol = "Grid Symbol";
        public const string GridResource = "Grid Resource";
    }
    public struct Layer
    {
        public const string TechUI = "TechUI";
        public const string TechUIHandle = "TechUIHandle";
    }
}