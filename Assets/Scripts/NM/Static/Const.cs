namespace NM;

public static class Const
{
    public const string ProjName = "NM";
    
    public const int SpinW = 5;
    public const int SpinH = 4;
    public const int SpinFirstID = 1;
    public const int DeckMax = 20;
    public const int GridSize = 1;

    // 字段名必须以Tag结尾!
    public static class AddrResTag
    {
        public const string ConfigTag = "NMConfig";
        public const string ArtTag = "NMArt";
    }

    public static class SaveName
    {
        public const string SlotFolder = "Slot";
        public const string SettingFolder = "Setting";
        public const string SettingName = "Setting";
    }
    
    public static class Property
    {
        public const string Name1 = "体魄";
        public const string Name2 = "理智";
        public const string Name3 = "智识";
    }

    public static class TMPResource
    {
        
    }

    public static class Layer
    {
        public const string TechUI = "TechUI";
        public const string TechUIHandle = "TechUIHandle";
    }
}