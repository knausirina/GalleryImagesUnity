using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Zenject;

public class GalleryController : IInitializable, IDisposable
{
    private readonly IGalleryView _view;
    private readonly Config _config;
    private readonly SignalBus _signalBus;
    private readonly ImageProvider _imageProvider;
    private readonly CancellationTokenSource _cts = new();

    private readonly float _fadeDuration = 0.25f;
    private readonly HashSet<int> _activeLoads = new();

    private readonly Dictionary<int, CancellationTokenSource> _slotCts = new();

    private CancellationTokenSource _filterCts;
    private bool _isThrottling;

    public GalleryController(SignalBus signalBus, ImageProvider imageProvider, IGalleryView view, Config config)
    {
        _signalBus = signalBus;
        _imageProvider = imageProvider;
        _view = view;
        _config = config;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<FilterChangedSignal>(OnFilterChanged);
        _view.OnScrollChanged += OnScrollWithThrottle;

        SetupGalleryAsync(_cts.Token).Forget();
    }

    public void Dispose()
    {
        _view.OnScrollChanged -= OnScrollWithThrottle;
        _cts.Cancel();
        _cts.Dispose();
        _filterCts?.Cancel();
        _filterCts?.Dispose();
        _signalBus.Unsubscribe<FilterChangedSignal>(OnFilterChanged);
    }

    private async UniTaskVoid SetupGalleryAsync(CancellationToken token)
    {
        _view.CreatePlaceholders(_config.TotalImages);
        await _view.RefreshLayoutAsync(token);
        CheckVisibleSlots();
    }

    private void OnScrollWithThrottle()
    {
        if (_isThrottling)
            return;
        ApplyThrottleAsync().Forget();
    }

    private async UniTaskVoid ApplyThrottleAsync()
    {
        _isThrottling = true;
        try
        {
            CheckVisibleSlots();
            await UniTask.DelayFrame(5, cancellationToken: _cts.Token);
        }
        finally
        {
            _isThrottling = false;
        }
    }

    private void CheckVisibleSlots()
    {
        for (int i = 0; i < _config.TotalImages; i++)
        {
            bool isVisible = _view.IsVisible(i);

            if (isVisible)
            {
                if (!_view.IsSlotBusy(i) && !_activeLoads.Contains(i))
                {
                    LoadSlot(i, _cts.Token).Forget();
                }
            }
            else
            {
                _view.ClearSlot(i);
                if (_slotCts.TryGetValue(i, out var slotCts))
                {
                    try { slotCts.Cancel(); } catch { }
                }
            }
        }
    }

    private async UniTaskVoid LoadSlot(int index, CancellationToken token)
    {
        _activeLoads.Add(index);
        _view.PrepareSlotForLoading(index);

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _slotCts[index] = linkedCts;
        var slotToken = linkedCts.Token;

        try
        {
            var isPremium = IsPremium(index);
            string url = _config.GetUrlImage(index);
            _view.SetData(index, isPremium);

            var sprite = await _imageProvider.GetSpriteAsync(url, slotToken);

            if (!slotToken.IsCancellationRequested && _view.IsVisible(index))
            {
                _view.SetPhotoToSlot(index, sprite, isPremium);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            if (!slotToken.IsCancellationRequested)
                HandleLoadError(index);
        }
        finally
        {
            _activeLoads.Remove(index);
            if (_slotCts.TryGetValue(index, out var existing) && existing == linkedCts)
            {
                _slotCts.Remove(index);
            }

            try { linkedCts.Dispose(); } catch { }
        }

    }

    private void OnFilterChanged(FilterChangedSignal signal)
    {
        _filterCts?.Cancel();
        _filterCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        ChangeFilterAsync(signal.Filter, _filterCts.Token).Forget();
    }

    private async UniTaskVoid ChangeFilterAsync(GalleryFilter filter, CancellationToken token)
    {
        try
        {
            await _view.FadeAsync(0, _fadeDuration, token);

            for (int i = 0; i < _config.TotalImages; i++)
            {
                bool shouldShow = filter switch
                {
                    GalleryFilter.Odd => (i + 1) % 2 != 0,
                    GalleryFilter.Even => (i + 1) % 2 == 0,
                    _ => true
                };
                _view.ToggleSlot(i, shouldShow);
            }

            _view.ResetScrollPosition();
            await _view.RefreshLayoutAsync(token);
            await _view.FadeAsync(1, _fadeDuration, token);

            CheckVisibleSlots();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void HandleLoadError(int index)
    {
        _view.ShowErrorInSlot(index, () => LoadSlot(index, _cts.Token).Forget());
    }

    private bool IsPremium(int index) => (index + 1) % 4 == 0;
}