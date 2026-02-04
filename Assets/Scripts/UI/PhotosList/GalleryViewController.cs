using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GalleryViewController : MonoBehaviour, IGalleryView
{
    public event Action OnScrollChanged;

    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private CanvasGroup _contentGroup;

    private RectTransform[] _placeholders;
    private PhotoItem[] _activeItems;
    private PhotoPool _pool;

    private readonly Vector3[] _slotCorners = new Vector3[4];
    private readonly Vector3[] _viewCorners = new Vector3[4];

    public float _buffer;   

    [Inject]
    public void Construct(PhotoPool photoPool)
    {
        _pool = photoPool;
    }

    private void OnEnable()
    {
        _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        _buffer = Screen.height;
    }

    private void OnDisable()
    {
        _scrollRect.onValueChanged.RemoveAllListeners();
    }

    private void OnScrollValueChanged(Vector2 arg0)
    {
        OnScrollChanged.Invoke();
    }
    public void CreatePlaceholders(int count)
    {
        _placeholders = new RectTransform[count ];
        _activeItems = new PhotoItem[count];

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Slot_{i}", typeof(RectTransform));
            go.transform.SetParent(_scrollRect.content, false);
            go.transform.SetAsLastSibling();
            _placeholders[i] = go.GetComponent<RectTransform>();
        }
    }

    public void ShowErrorInSlot(int index, Action action)
    {
        if (_activeItems[index] != null)
        {
            _activeItems[index].ShowError(action);
        }
    }

    public bool IsVisible(int index)
    {
        if (index < 0 || index >= _placeholders.Length) return false;

        _placeholders[index].GetWorldCorners(_slotCorners);
        _scrollRect.viewport.GetWorldCorners(_viewCorners);

        return _slotCorners[2].y > (_viewCorners[0].y - _buffer) &&
                _slotCorners[0].y < (_viewCorners[2].y + _buffer);
    }

    public bool IsSlotBusy(int index) => _activeItems[index] != null;

    public void SetPhotoToSlot(int index, Sprite sprite, bool isPremium)
    {
        if (index < 0 || index >= _activeItems.Length) return;

        if (_activeItems[index] == null)
            PrepareSlotForLoading(index);

        _activeItems[index].SetData(index, sprite, isPremium);
    }

    public void SetData(int index, bool isPremium)
    {
        if (index < 0 || index >= _activeItems.Length) return;

        if (_activeItems[index] == null)
            PrepareSlotForLoading(index);

        _activeItems[index].SetData(index, null, isPremium);
    }

    public void PrepareSlotForLoading(int index)
    {
        if (_activeItems[index] != null)
            return;

        GameObject obj = _pool.Get(_placeholders[index]);
        _activeItems[index] = obj.GetComponent<PhotoItem>();

        _activeItems[index].Reset();
    }

    public void ClearSlot(int index)
    {
        if (_activeItems[index] == null)
            return;

        _pool.Return(_activeItems[index].gameObject);
        _activeItems[index] = null;
    }

    public void ToggleSlot(int index, bool active)
    {
        _placeholders[index].gameObject.SetActive(active);
    }

    public void ResetScrollPosition() => _scrollRect.verticalNormalizedPosition = 1f;

    public async UniTask RefreshLayoutAsync(CancellationToken token)
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
    }

    public async UniTask FadeAsync(float targetAlpha, float duration, CancellationToken token)
    {
        float startAlpha = _contentGroup.alpha;
        float elapsed = 0;

        if (duration <= 0)
        {
            _contentGroup.alpha = targetAlpha;
            return;
        }

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested) return;

            elapsed += Time.deltaTime;
            _contentGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);

            await UniTask.Yield(token);
        }

        _contentGroup.alpha = targetAlpha;
    }

    private void OnDestroy()
    {
        if (_activeItems == null)
            return;

        for (int i = 0; i < _activeItems.Length; i++)
        {
            if (_activeItems[i] != null )
                _pool.Return(_activeItems[i].gameObject);
        }
    }
}