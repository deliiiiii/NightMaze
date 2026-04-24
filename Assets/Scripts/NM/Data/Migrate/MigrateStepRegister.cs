using System;
using General;
using GeneralPreview;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;

namespace NM.Data;

public static class MigrateStepRegister
{
    public static void Init()
    {
        MigrateStepFactory<JObject, GamePlaying>.Clear();
        typeof(IMigrateStepJson).SubTypes().ForEach(type =>
        {
            MigrateStepFactory<JObject, GamePlaying>.Add((IMigrateStepJson)Activator.CreateInstance(type));
        });
    }
}