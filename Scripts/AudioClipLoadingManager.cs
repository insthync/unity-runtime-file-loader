using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RuntimeFileLoader
{
    public class AudioClipLoadingManager
    {
        private static readonly Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

        public static async void GetAudioClip(string url, Action<AudioClip> onLoaded, int attempts = 10)
        {
            AudioClip clip = await GetAudioClip(url, attempts);
            if (onLoaded != null)
                onLoaded.Invoke(clip);
        }

        public static async Task<AudioClip> GetAudioClip(string url, int attempts = 10)
        {
            int attempsCount = 0;
            if (audioClips.ContainsKey(url))
            {
                return audioClips[url];
            }
            else
            {
                AudioType audioType = AudioType.MPEG;
                string extension = url.Substring(url.LastIndexOf(".")).ToLower();
                if (extension.StartsWith(".ogg"))
                    audioType = AudioType.OGGVORBIS;
                if (extension.StartsWith(".wav"))
                    audioType = AudioType.WAV;
                UnityWebRequest www;
                do
                {
#if UNITY_EDITOR
                    float startTime = Time.unscaledTime;
#endif
                    www = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
                    UnityWebRequestAsyncOperation asyncOp = www.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        await Task.Yield();
                    }
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("[AudioClipFromUrlLoader] Network Error: " + www.error);
                        if (www.error.Equals("Malformed URL"))
                            audioClips[url] = null;
                        attempsCount++;
                    }
                    else
                    {
                        DownloadHandlerAudioClip downloadHandler = (DownloadHandlerAudioClip)www.downloadHandler;
#if UNITY_EDITOR
                        Debug.Log("Audio loaded from " + url + " size " + downloadHandler.data.Length.ToString("N0") + " duration " + (Time.unscaledTime - startTime));
#endif
                        audioClips[url] = downloadHandler.audioClip;
                        return audioClips[url];
                    }
                } while (attempsCount < attempts);
            }
            return null;
        }
    }
}
