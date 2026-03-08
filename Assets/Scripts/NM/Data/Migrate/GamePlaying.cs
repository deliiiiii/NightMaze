using General;
using Newtonsoft.Json.Linq;

namespace NM.Data;

public class MGamePlaying_20260308 : IMigrateStepJson<GamePlaying>
{
    public double FromVersion => 20260308;
    public double ToVersion => 20260308.1;
    public JObject Migrate(JObject data)
    {
        data["TestAdded"] = 1;
        return data;
    }
}