using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RuntimeFileLoader
{
    public class TextureLoadingManager
    {
        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        public static async void GetSprite(string url, Action<Sprite> onLoaded, int attempts = 10)
        {
            Sprite sprite = await GetSprite(url, attempts);
            if (onLoaded != null)
                onLoaded.Invoke(sprite);
        }

        public static async void GetTexture(string url, Action<Texture2D> onLoaded, int attempts = 10)
        {
            Texture2D texture = await GetTexture(url, attempts);
            if (onLoaded != null)
                onLoaded.Invoke(texture);
        }

        public static async Task<Sprite> GetSprite(string url, int attempts = 10)
        {
            Texture2D texture2D = await GetTexture(url, attempts);
            if (texture2D == null)
                sprites[url] = null;
            else
                sprites[url] = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one * 0.5f);
            return sprites[url];
        }

        public static async Task<Texture2D> GetTexture(string url, int attempts = 10)
        {
            int attempsCount = 0;
            if (textures.ContainsKey(url))
            {
                return textures[url];
            }
            else
            {
                UnityWebRequest www;
                do
                {
#if UNITY_EDITOR
                    float startTime = Time.unscaledTime;
#endif
                    www = UnityWebRequestTexture.GetTexture(url);
                    UnityWebRequestAsyncOperation asyncOp = www.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        await Task.Yield();
                    }
                    if (www.result != UnityWebRequest.Result.Success)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogError($"[{nameof(TextureLoadingManager)}] Network Error: {www.error} from URL: {url}");
#endif
                        if (www.error.Equals("Malformed URL"))
                            textures[url] = null;
                        attempsCount++;
                    }
                    else
                    {
                        DownloadHandlerTexture downloadHandler = (DownloadHandlerTexture)www.downloadHandler;
#if UNITY_EDITOR
                        Debug.Log("Texture loaded from " + url + " size " + downloadHandler.data.Length.ToString("N0") + " duration " + (Time.unscaledTime - startTime));
#endif
                        textures[url] = ((DownloadHandlerTexture)www.downloadHandler).texture;
                        return textures[url];
                    }
                } while (attempsCount < attempts);
            }
            return null;
        }
    }
}
