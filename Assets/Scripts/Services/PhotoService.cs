    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
   using System;

    public class PhotoService : IDisposable
    {
        private readonly ImageProvider _imageProvider;
        private readonly Dictionary<string, Sprite> _spriteCache = new();

        public PhotoService(ImageProvider imageProvider)
        {
            _imageProvider = imageProvider;
        }

        public async UniTask<Sprite> GetSprite(string url, CancellationToken token)
        {
            if (_spriteCache.TryGetValue(url, out var cachedSprite) && cachedSprite != null)
            {
                return cachedSprite;
            }
            Texture2D rawTexture = await _imageProvider.GetTextureAsync(url, token);

            Sprite sprite = Sprite.Create(rawTexture, new Rect(0, 0, rawTexture.width, rawTexture.height), Vector2.one * 0.5f);
            
            _spriteCache[url] = sprite;
            return sprite;
        }

        public void Dispose()
        {
            _spriteCache.Clear();
        }
    }