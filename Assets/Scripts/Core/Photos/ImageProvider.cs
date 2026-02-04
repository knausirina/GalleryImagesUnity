    using Cysharp.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.Networking;

    public class ImageProvider : IDisposable
    {
        private readonly Dictionary<string, UniTaskCompletionSource<Sprite>> _loadingSources = new();
        private readonly Dictionary<string, Sprite> _cache = new();
        private readonly BypassCertificate _bypassCertificate = new BypassCertificate();

        private const int MaxTriesCount = 3;
        private const int Timeout = 20;

        public async UniTask<Sprite> GetSpriteAsync(string url, CancellationToken token)
        {
            if (_cache.TryGetValue(url, out var sprite)) return sprite;

            if (_loadingSources.TryGetValue(url, out var tcs))
            {
                return await tcs.Task.AttachExternalCancellation(token);
            }

            tcs = new UniTaskCompletionSource<Sprite>();
            _loadingSources[url] = tcs;

            try
            {
                var result = await DownloadImageInternal(url, token);
                _cache[url] = result;
                tcs.TrySetResult(result);
                return result;
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled();
                throw;
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
                throw;
            }
            finally
            {
                _loadingSources.Remove(url);
            }
        }

        public void Dispose()
        {
            foreach (var source in _loadingSources.Values)
                source.TrySetCanceled();

            foreach (var sprite in _cache.Values)
            {
                if (sprite != null)
                {
                    if (sprite.texture != null) UnityEngine.Object.Destroy(sprite.texture);
                    UnityEngine.Object.Destroy(sprite);
                }
            }
            _cache.Clear();
            _loadingSources.Clear();
            _bypassCertificate.Dispose();
        }

        private async UniTask<Sprite> DownloadImageInternal(string url, CancellationToken token)
        {
            for (int i = 0; i < MaxTriesCount; i++)
            {
                try
                {
                    using var request = UnityWebRequestTexture.GetTexture(url);
                    request.timeout = Timeout;
                    request.certificateHandler = _bypassCertificate;

                    await request.SendWebRequest().ToUniTask(cancellationToken: token);

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var handler = (DownloadHandlerTexture)request.downloadHandler;
                        Texture2D texture = handler.texture;
                        if (texture == null)
                            throw new Exception("Texture data is empty");

                        texture.wrapMode = TextureWrapMode.Clamp;
                        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    }

                    throw new Exception($"UnityWebRequest error (code {request.responseCode}): {request.error}");
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception e)
                {
                    if (i == MaxTriesCount - 1)
                    {
                        Debug.LogError($"[ImageProvider] Final attempt failed for {url}: {e.Message}");
                        throw;
                    }

                    Debug.LogWarning($"[ImageProvider] Retry {i + 1} for {url} due to: {e.Message}");
                    await UniTask.Delay(1000, cancellationToken: token);
                }
            }

            throw new Exception($"Failed to download image from {url} after {MaxTriesCount} attempts.");
        }
    }