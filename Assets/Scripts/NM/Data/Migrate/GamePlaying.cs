using General;
using Newtonsoft.Json.Linq;

namespace NM.Data;

public class MGamePlaying_20260308 : IMigrateStepJson<GamePlaying>
{
    public double FromVersion => 20260308;
    public double ToVersion => 20260308.1;
    public JObject Migrate(JObject data)
    {
        data["PlayTimex2"] = data["PlayTime"]?.Value<float>() * 2;
        return data;
    }
}

public class MGamePlaying_20260308d1 : IMigrateStepJson<GamePlaying>
{
    public double FromVersion => 20260308.1;
    public double ToVersion => 20260308.2;
    public JObject Migrate(JObject data)
    {
        var playTime2 = data["PlayTimex2"]?.Value<float>();
        data["Name2"] = "Name2..." + (playTime2?.ToString("F2") ?? "null");
        return data;
    }
}