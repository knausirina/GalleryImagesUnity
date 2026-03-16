using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PhotoLoader : IDisposable
{
    private readonly IGalleryView _view;
    private readonly PhotoService _photoService;
    private readonly Config _config;

    private readonly CancellationTokenSource _cts = new();
    private readonly Dictionary<int, CancellationTokenSource> _activeLoads = new();
    private readonly SemaphoreSlim _concurrentLoads = new(5);

    private const int AdditionalPhotoCount = 2;

    public PhotoLoader(IGalleryView view, PhotoService photoService, Config config)
    {
        _view = view;
        _photoService = photoService;
        _config = config;
    }

    public void CheckVisibleSlots(IReadOnlyList<int> filteredIndices, CancellationToken token)
    {
        var (start, end) = _view.GetVisibleRange();

        var activeViewIndexes = _activeLoads.Keys.ToList();
        foreach (var viewIndex in activeViewIndexes)
        {
            if (viewIndex < start - AdditionalPhotoCount || viewIndex > end + AdditionalPhotoCount)
            {
                CancelSlotLoad(viewIndex);
                _view.ReleaseSlot(viewIndex);
            }
        }

        var bufferStart = Mathf.Max(0, start - AdditionalPhotoCount);
        var bufferEnd = Mathf.Min(filteredIndices.Count - 1, end + AdditionalPhotoCount);

        for (var viewIndex = bufferStart; viewIndex <= bufferEnd; viewIndex++)
        {
            if (_activeLoads.ContainsKey(viewIndex) || _view.IsSlotBusy(viewIndex))
                continue;

            var dataIndex = filteredIndices[viewIndex];
            _view.PrepareSlot(viewIndex);
            LoadSlot(viewIndex, dataIndex, token).Forget();
        }
    }

    private async UniTaskVoid LoadSlot(int viewIndex, int dataIndex, CancellationToken token)
    {
        var slotCts = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
        _activeLoads[viewIndex] = slotCts;

        var isAcquired = false;
        try
        {
            await _concurrentLoads.WaitAsync(slotCts.Token);
            isAcquired = true;

            _view.PrepareSlotForLoading(viewIndex);

            var url = _config.GetUrlImage(dataIndex);
            var isPremium = (dataIndex + 1) % 4 == 0;

            _view.SetData(viewIndex, isPremium);

            var sprite = await _photoService.GetSprite(url, slotCts.Token);

            if (!slotCts.IsCancellationRequested)
            {
                _view.SetPhotoToSlot(viewIndex, sprite, isPremium);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            Debug.LogError($"Error loading view slot {viewIndex} (data index {dataIndex}): {e.Message}");
            _view.ShowErrorInSlot(viewIndex, null);
        }
        finally
        {
            if (isAcquired)
                _concurrentLoads.Release();

            if (_activeLoads.TryGetValue(viewIndex, out var current) && current == slotCts)
            {
                _activeLoads.Remove(viewIndex);
            }

            slotCts.Dispose();
        }
    }

    public void StopAllLoads()
    {
        foreach (var cts in _activeLoads.Values)
        {
            cts.Cancel();
        }
        _activeLoads.Clear();
    }

    private void CancelSlotLoad(int viewIndex)
    {
        if (_activeLoads.Remove(viewIndex, out var cts))
        {
            cts.Cancel();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        StopAllLoads();
        _concurrentLoads.Dispose();
    }
}