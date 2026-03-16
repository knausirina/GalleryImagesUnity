using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class ImageProvider : IDisposable
{
    private readonly Dictionary<string, UniTask<Texture2D>> _loadingTasks = new();
    private readonly Dictionary<string, Texture2D> _textureCache = new();
    private const int MaxTriesCount = 3;
    private const int TimeoutSeconds = 20;

    public async UniTask<Texture2D> GetTextureAsync(string url, CancellationToken token)
    {
        if (_textureCache.TryGetValue(url, out var cachedTex) && cachedTex != null)
            return cachedTex;

        if (_loadingTasks.TryGetValue(url, out var loadingTask))
            return await loadingTask.AttachExternalCancellation(token);

        var task = DownloadImageInternal(url, token);
        _loadingTasks[url] = task;

        try
        {
            var texture = await task;
            _textureCache[url] = texture;
            return texture;
        }
        finally
        {
            _loadingTasks.Remove(url);
        }
    }

    private async UniTask<Texture2D> DownloadImageInternal(string url, CancellationToken token)
    {
        for (var i = 0; i < MaxTriesCount; i++)
        {
            try
            {
                using var request = new UnityWebRequest(url);
                var handler = new DownloadHandlerTexture(false);
                request.downloadHandler = handler;
                request.timeout = TimeoutSeconds;

                await request.SendWebRequest().ToUniTask(cancellationToken: token);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var texture = handler.texture;
                    if (texture == null)
                        throw new Exception("Downloaded texture is null");

                    texture.wrapMode = TextureWrapMode.Clamp;
                    
                    return texture;
                }

                throw new Exception($"Network error: {request.error} (Code: {request.responseCode})");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e)
            {
                if (i == MaxTriesCount - 1)
                {
                    Debug.LogError($"[ImageProvider] Failed to load {url} after {MaxTriesCount} tries. Error: {e.Message}");
                    throw;
                }

                Debug.LogWarning($"[ImageProvider] Retry {i + 1} for {url}...");
                await UniTask.Delay(1000, cancellationToken: token);
            }
        }

        throw new Exception("Unknown download error");
    }

    public void Dispose()
    {
        foreach (var tex in _textureCache.Values)
        {
            if (tex != null)
            UnityEngine.Object.Destroy(tex);
        }
        
        _textureCache.Clear();
        _loadingTasks.Clear();
    }
}