using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace General
{
    public static class Saver
    {
        public static void Save<T>(string pathPre, string name, T curEntity)
        {
            JsonIO.Write(pathPre, name, curEntity);
        }
        public static async UniTask<T> LoadAsync<T>(string pathPre, string name, CancellationToken ct)
        {
            await UniTask.SwitchToThreadPool();
            var ret = await JsonIO.ReadAsync<T>(pathPre, name, ct);
            await UniTask.SwitchToMainThread();
            return ret;
        }

        public static async UniTask<List<T>> LoadAllAsync<T>(string pathPre, CancellationToken ct)
        {
            await UniTask.SwitchToThreadPool();
            var resultList = new List<T>();
            if (Directory.Exists(pathPre))
            {
                string[] files = Directory.GetFiles(pathPre, "*.json");

                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    var data = await JsonIO.ReadAsync<T>(pathPre, fileName, ct);
                    if (data != null)
                    {
                        resultList.Add(data);
                    }
                }
            }
            await UniTask.SwitchToMainThread();
            return resultList;
        }
        public static void Delete(string pathPre, string name)
        {
            JsonIO.Delete(pathPre, name);
        }
    }
}