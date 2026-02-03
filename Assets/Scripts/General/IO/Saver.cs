using JetBrains.Annotations;

public static class Saver
{
    public static void Save<T>(string pathPre, string name, T curEntity)
    {
        JsonIO.Write(pathPre,name,curEntity);
    }
    [CanBeNull]
    public static T Load<T>(string pathPre,string name)
    {
        return JsonIO.Read<T>(pathPre,name);
    }
    public static void Delete(string pathPre,string name)
    {
        JsonIO.Delete(pathPre,name);
    }
}