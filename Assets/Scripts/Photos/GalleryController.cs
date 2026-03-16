using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Zenject;

public class GalleryController : IInitializable, IDisposable
{
    private readonly IGalleryView _view;
    private readonly SignalBus _signalBus;
    private readonly GalleryFilterManager _filterManager;
    private readonly PhotoLoader _photoLoader;

    private readonly CancellationTokenSource _cts = new();
    private const int ThrottleMs = 100;
    private bool _isThrottling;

    public GalleryController(SignalBus signalBus, IGalleryView view, GalleryFilterManager filterManager, PhotoLoader photoLoader)
    {
        _signalBus = signalBus;
        _view = view;
        _filterManager = filterManager;
        _photoLoader = photoLoader;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<FilterChangedSignal>(OnFilterChanged);
        _view.OnScrollChanged += OnScrollWithThrottle;

        _view.CreatePlaceholders(_filterManager.GetFilteredIndices().Count);
        _photoLoader.CheckVisibleSlots(_filterManager.GetFilteredIndices(), _cts.Token);
    }

    public void Dispose()
    {
        _view.OnScrollChanged -= OnScrollWithThrottle;
        _signalBus.TryUnsubscribe<FilterChangedSignal>(OnFilterChanged);

        _photoLoader.Dispose();
        _cts.Cancel();
        _cts.Dispose();
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
            _photoLoader.CheckVisibleSlots(_filterManager.GetFilteredIndices(), _cts.Token);
            await UniTask.Delay(ThrottleMs, cancellationToken: _cts.Token);
        }
        catch (OperationCanceledException) { }
        finally { _isThrottling = false; }
    }

    private void OnFilterChanged(FilterChangedSignal signal)
    {
        _filterManager.SetFilter(signal.Type);

        _photoLoader.StopAllLoads();

        _view.ClearAllSlots();
        _view.CreatePlaceholders(_filterManager.GetFilteredIndices().Count);
        _view.ResetScrollPosition();

        _photoLoader.CheckVisibleSlots(_filterManager.GetFilteredIndices(), _cts.Token);
    }
}