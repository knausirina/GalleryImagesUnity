using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BannersView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private LayoutElement[] _layoutElements;
    [SerializeField] private PaginationView _pagination;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _content;
    [SerializeField] private Canvas _canvas;

    private const int BannersCount = 3;
    private const float AutoScrollDelay = 5f;
    private const float SnapSpeed = 15f;
    private const float Threshold = 0.05f;

    private int _currentIndex = 1;
    private float[] _positions;
    private bool _isDragging = false;
    private float _timer;

    private void Start()
    {
        var canvasRect = _canvas.GetComponent<RectTransform>();
        var width = canvasRect.rect.width;
        foreach (var layoutElement in _layoutElements)
        {
            layoutElement.preferredWidth = width;
        }
        
        const int totalElements = BannersCount + 2;
        _positions = new float[totalElements];
        const float step = 1f / (totalElements - 1);
        for (var i = 0; i < totalElements; i++) _positions[i] = step * i;

        _scrollRect.horizontalNormalizedPosition = _positions[_currentIndex];
        _timer = AutoScrollDelay;
    }

    private void Update()
    {
        if (_isDragging)
            return;

        _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            _scrollRect.horizontalNormalizedPosition,
            _positions[_currentIndex],
            Time.deltaTime * SnapSpeed);

        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _currentIndex++;
            _timer = AutoScrollDelay;
        }

        CheckBoundary();
        UpdatePaginationCall();
    }

    private void UpdatePaginationCall()
    {
        if (_pagination == null)
            return;
        var visualIndex = _currentIndex - 1;
        if (visualIndex < 0)
            visualIndex = BannersCount - 1;
        if (visualIndex >= BannersCount)
            visualIndex = 0;
        _pagination.SetActivePage(visualIndex);
    }

    private void CheckBoundary()
    {
        if (Mathf.Abs(_scrollRect.horizontalNormalizedPosition - _positions[_currentIndex]) < 0.01f)
        {
            if (_currentIndex > BannersCount)
            {
                _currentIndex = 1;
                _scrollRect.horizontalNormalizedPosition = _positions[_currentIndex];
            }
            else if (_currentIndex < 1)
            {
                _currentIndex = BannersCount;
                _scrollRect.horizontalNormalizedPosition = _positions[_currentIndex];
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _timer = AutoScrollDelay;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        _timer = AutoScrollDelay;

        var dragDistance = eventData.pressPosition.x - eventData.position.x;
        var dragPercent = dragDistance / Screen.width;

        if (Mathf.Abs(dragPercent) > Threshold)
        {
            if (dragPercent > 0) _currentIndex++;
            else _currentIndex--;
        }

        _currentIndex = Mathf.Clamp(_currentIndex, 0, _positions.Length - 1);
    }
}