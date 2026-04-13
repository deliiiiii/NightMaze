using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace General
{
    public static class Saver
    {
        public static UniTask SaveAsync<T>(string pathPre, string name, T curEntity) 
            => JsonIO.WriteAsync(pathPre, name, curEntity);
        public static T Load<T>(string pathPre, string name)
            => JsonIO.Read<T>(pathPre, name);
        public static UniTask<T> LoadAsync<T>(string pathPre, string name, CancellationToken ct)
            => JsonIO.ReadAsync<T>(pathPre, name, ct);
        public static UniTask<T> LoadWithVerAsync<T>(string pathPre, string name, CancellationToken ct) 
            where T : IHasVersion
            => JsonIO.ReadWithVerAsync<T>(pathPre, name, ct);
        public static async UniTask<List<T>> LoadAllAsync<T>(string pathPre, CancellationToken ct)
        {
            // await UniTask.SwitchToThreadPool();
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
            // await UniTask.SwitchToMainThread();
            return resultList;
        }
        public static async UniTask<List<T>> LoadAllWithVerAsync<T>(string pathPre, CancellationToken ct)
            where T : IHasVersion
        {
            // await UniTask.SwitchToThreadPool();
            var resultList = new List<T>();
            if (Directory.Exists(pathPre))
            {
                string[] files = Directory.GetFiles(pathPre, "*.json");

                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    var data = await JsonIO.ReadWithVerAsync<T>(pathPre, fileName, ct);
                    if (data != null)
                    {
                        resultList.Add(data);
                    }
                }
            }
            // await UniTask.SwitchToMainThread();
            return resultList;
        }
        
        
        public static void Delete(string pathPre, string name)
        {
            JsonIO.Delete(pathPre, name);
        }
    }
}