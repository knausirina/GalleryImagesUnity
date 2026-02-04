using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PhotoItem : MonoBehaviour
{
    [SerializeField] GameObject _premiumGO;
    [SerializeField] private Image _image;
    [SerializeField] private Button _retryButton;
    [SerializeField] private GameObject _loadingIndicator;
    public Button Button { get; private set; }

    private Action _onRetry;
    private SignalBus _signalBus;
    private bool _isPremium;
    private int _index;
    private RectTransform _rectTransform;

    [Inject]
    private void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void SetData(int index, Sprite sprite, bool isPremium)
    {
        _index = index;
        _isPremium = isPremium;
        _premiumGO.SetActive(isPremium);

        _image.sprite = sprite;

        if (sprite != null)
        {
            _image.gameObject.SetActive(true);
            _image.canvasRenderer.SetAlpha(0f);
           _image.CrossFadeAlpha(1, 0.3f, false);

            _loadingIndicator.SetActive(false);
        }
        else
        {
            _image.gameObject.SetActive(false);
            _loadingIndicator.SetActive(true);
        }

        _retryButton.gameObject.SetActive(false);
    }

    public void ShowError(Action onRetry)
    {
        _onRetry = onRetry;
        _loadingIndicator.SetActive(false);
        _retryButton.gameObject.SetActive(true);
        _image.gameObject.SetActive(false);
    }

    public void Reset()
    {
        _premiumGO.SetActive(false);
        
        _image.canvasRenderer.SetAlpha(1f);
        _image.sprite = null;

        _retryButton.gameObject.SetActive(false);
        _loadingIndicator.SetActive(true);

        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();
       
        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.one;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }

    private void Start()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(() =>
        {
            _signalBus.Fire(new PhotoClickedSignal(_index, _isPremium));
        });

        _retryButton.onClick.AddListener(HandleRetry);
    }

    private void HandleRetry()
    {
        _retryButton.gameObject.SetActive(false);
        _loadingIndicator.SetActive(true);
        _onRetry?.Invoke();
    }

    private void OnDestroy()
    {
        if (Button != null)
            Button.onClick.RemoveAllListeners();
        _retryButton.onClick.RemoveAllListeners();
    }
}