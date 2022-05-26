using Siccity.GLTFUtility;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RuntimeFileLoader
{
    public class GLTFLoadingManager
    {
        private static readonly Dictionary<string, byte[]> modelBytes = new Dictionary<string, byte[]>();

        public static async Task<GameObject> GetModel(string url, int attempts = 10)
        {
            byte[] modelBytes = await GetModelBytes(url, attempts);
            return Importer.LoadFromBytes(modelBytes);
        }

        public static async Task<byte[]> GetModelBytes(string url, int attempts = 10)
        {
            int attempsCount = 0;
            if (modelBytes.ContainsKey(url))
            {
                return modelBytes[url];
            }
            else
            {
                UnityWebRequest www;
                do
                {
#if UNITY_EDITOR
                    float startTime = Time.unscaledTime;
#endif
                    www = UnityWebRequest.Get(url);
                    UnityWebRequestAsyncOperation asyncOp = www.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        await Task.Yield();
                    }
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[{nameof(GLTFLoadingManager)}] Network Error: {www.error}");
                        if (www.error.Equals("Malformed URL"))
                            modelBytes[url] = null;
                        attempsCount++;
                    }
                    else
                    {
                        DownloadHandler downloadHandler = www.downloadHandler;
#if UNITY_EDITOR
                        Debug.Log("Mesh loaded from " + url + " size " + downloadHandler.data.Length.ToString("N0") + " duration " + (Time.unscaledTime - startTime));
#endif
                        modelBytes[url] = downloadHandler.data;
                        return modelBytes[url];
                    }
                } while (attempsCount < attempts);
            }
            return null;
        }
    }
}
