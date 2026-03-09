using General;
using Newtonsoft.Json.Linq;

namespace NM.Data;

public static class MigrateStepRegister
{
    public static void Init()
    {
        MigrateStepFactory<JObject, GamePlaying>.Clear();
        MigrateStepFactory<JObject, GamePlaying>.Add(new MGamePlaying_20260308());
        MigrateStepFactory<JObject, GamePlaying>.Add(new MGamePlaying_20260308d1());
        MigrateStepFactory<JObject, GamePlaying>.Add(new MGamePlaying_20260308d2());
    }
}