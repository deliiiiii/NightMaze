using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace General
{
    public interface IHasVersion
    {
        // ReSharper disable once InconsistentNaming
        double savedVersion { get; set; }
    }
    public interface IMigrateStep<TDiskData, TRuntimeData>
        where TRuntimeData : IHasVersion
    {
        double FromVersion { get; }
        double ToVersion { get; }
        TDiskData Migrate(TDiskData data);
    }

    public interface IMigrateStepJson<TRuntimeData> : IMigrateStep<JObject, TRuntimeData>
        where TRuntimeData : IHasVersion
    {}

    public class MigrateStepFactory<TDiskData, TRuntimeData>
        where TDiskData : class
        where TRuntimeData : IHasVersion
    {
        static readonly Dictionary<double, IMigrateStep<TDiskData, TRuntimeData>> stepDic = new();
        public static void Clear() => stepDic.Clear();
        public static void Add(IMigrateStep<TDiskData, TRuntimeData> step)
        {
            if (!stepDic.TryAdd(step.FromVersion, step))
            {
                MyDebug.LogError($"已存在从版本{step.FromVersion:F2}开始的迁移步骤");
            }
        }

        [CanBeNull]
        public static TDiskData MigrateUntilCur(TDiskData data)
        {
            double curVersion = -1;
            if (data is JObject jObject)
            {
                var nullableVersion = jObject[Const.SavedVersionName]?.Value<double>();
                if (nullableVersion == null)
                {
                    MyDebug.LogError($"无法获取存档类型{typeof(TDiskData)}的数据{typeof(TRuntimeData)}的版本号");
                    return null;
                }
                curVersion = nullableVersion.Value;
            }
            else if (data is IHasVersion hasVersion)
            {
                curVersion = hasVersion.savedVersion;
            }

            if (Math.Abs(curVersion + 1) < 1e-6)
            {
                MyDebug.LogError($"存档类型{typeof(TDiskData)}应是JObject, 或实现IHasVersion接口");
                return null;
            }
            while (Math.Abs(curVersion - Const.Version) > 1e-2)
            {
                var step = stepDic.GetValueOrDefault(curVersion);
                if (step == null)
                {
                    MyDebug.LogError($"迁移存档类型{typeof(TDiskData)}的数据{typeof(TRuntimeData)}失败: 未找到从版本{curVersion}开始的迁移步骤. 将停留在该版本.");
                    return data;
                }
                data = stepDic[curVersion].Migrate(data);
                if (data is JObject jObject2)
                    jObject2[Const.SavedVersionName] = curVersion = stepDic[curVersion].ToVersion;
                else
                    ((IHasVersion)data).savedVersion = curVersion = stepDic[curVersion].ToVersion;
            }

            return data;
        }
    }
}