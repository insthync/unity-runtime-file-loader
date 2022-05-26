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
                    www = UnityWebRequestTexture.GetTexture(url);
                    await www.SendWebRequest();
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[{nameof(TextureLoadingManager)}] Network Error: {www.error}");
                        if (www.error.Equals("Malformed URL"))
                            textures[url] = null;
                        attempsCount++;
                    }
                    else
                    {
                        textures[url] = ((DownloadHandlerTexture)www.downloadHandler).texture;
                        return textures[url];
                    }
                } while (attempsCount < attempts);
            }
            return null;
        }
    }
}
