using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SimplePhotoPopup : Popup
{
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _loader;
    [SerializeField] private Button _retryButton;

    private ImageProvider _imageProvider;
    private Config _config;
    private CancellationTokenSource _lastLoadCts;
    private int _currentIndex;

    [Inject]
    private void Construct(ImageProvider imageProvider, Config config)
    {
        _imageProvider = imageProvider;
        _config = config;
    }

    private void Start()
    {
        if (_retryButton != null)
        {
            _retryButton.onClick.AddListener(OnRetryClick);
            _retryButton.gameObject.SetActive(false);
        }
    }

    public void SetData(int index)
    {
        _currentIndex = index;
        StartLoading();
    }

    private void OnRetryClick()
    {
        _retryButton.gameObject.SetActive(false);
        StartLoading();
    }

    private void StartLoading()
    {
        _lastLoadCts?.Cancel();
        _lastLoadCts?.Dispose();

        _lastLoadCts = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);

        LoadData(_currentIndex, _lastLoadCts.Token).Forget();
    }

    private async UniTask LoadData(int index, CancellationToken token)
    {
        _image.enabled = false;
        _image.sprite = null;
        _loader.SetActive(true);
        _retryButton.gameObject.SetActive(false);

        try
        {
            var url = _config.GetUrlImage(index);
            var sprite = await _imageProvider.GetSpriteAsync(url, token);

            _image.sprite = sprite;
            _image.enabled = true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Debug.LogError($"[SimplePhotoPopup] {e.Message}");
            if (!token.IsCancellationRequested)
            {
                _retryButton.gameObject.SetActive(true);
            }
        }
        finally
        {
            if (_loader != null) _loader.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_retryButton != null)
            _retryButton.onClick.RemoveAllListeners();
        _lastLoadCts?.Cancel();
        _lastLoadCts?.Dispose();
    }
}