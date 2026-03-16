    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Zenject;

    public class GalleryViewController : MonoBehaviour, IGalleryView
    {
        public event Action OnScrollChanged;

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private  RectTransform _content;
        
        private float _cellHeight;
        private float _cellWidth;
        private int _constraintCount;

        private PhotoPool _pool;
        
        private readonly Dictionary<int, PhotoItem> _activeItems = new();
        private readonly Dictionary<int, RectTransform> _activePlaceholders = new();
        
        private int _totalItemsCount;

        private float _buffer;

        private const int MobileColumnsCount = 2;
        private const int WideMobileColumnsCount = 3;
        private const float SpacingX = 38;
        private const float SpacingY = 40;

        private void Awake()
        {
            _constraintCount = DetecterTablet.IsTablet() ? WideMobileColumnsCount : MobileColumnsCount;
            
            var parentWidth = _content.rect.width;
            const int totalPaddings = 61 * 2;
            var totalSpacings = SpacingX * (_constraintCount - 1);
            var availableWidth = parentWidth - totalPaddings - totalSpacings;

            _cellWidth = availableWidth / _constraintCount;
            _cellHeight = _cellWidth;
        }

        [Inject]
        public void Construct(PhotoPool photoPool)
        {
            _pool = photoPool;
        }
        
        private void UpdateContentSize(int totalItems)
        {
            var rows = Mathf.CeilToInt((float)totalItems / _constraintCount);
            var totalHeight = rows * (_cellHeight + SpacingY);
            
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalHeight);
            
            _content.anchorMin = new Vector2(0.5f, 1f);
            _content.anchorMax = new Vector2(0.5f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
        }
        
        public void PrepareSlot(int index)
        {
            if (_activePlaceholders.ContainsKey(index))
                return;

            var placeholder = _pool.GetPlaceholder(); 
            placeholder.SetParent(_content, false);
            placeholder.sizeDelta = new Vector2(_cellWidth, _cellHeight);
            
            var row = index / _constraintCount;
            var col = index % _constraintCount;
            
            var x = (col - (_constraintCount - 1) * 0.5f) * (_cellWidth + SpacingX);
            var y = -row * (_cellHeight + SpacingY) - (_cellHeight * 0.5f);
            
            placeholder.anchoredPosition = new Vector2(x, y);
            _activePlaceholders[index] = placeholder;
        }

        public void ReleaseSlot(int index)
        {
            if (_activeItems.Remove(index, out var photoItem))
            {
                _pool.Return(photoItem.gameObject);
            }

            if (_activePlaceholders.Remove(index, out var placeholder))
            {
                _pool.ReturnPlaceholder(placeholder);
            }
        }

        private void OnEnable()
        {
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

            _buffer = _cellHeight * 3;
        }

        private void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveAllListeners();
        }

        private void OnScrollValueChanged(Vector2 arg0)
        {
            OnScrollChanged?.Invoke();
        }

        public void ClearAllSlots()
        {
            var keys = new List<int>(_activePlaceholders.Keys);
            foreach (var id in keys)
            {
                ClearSlot(id);
            }
        }
        
        public (int start, int end) GetVisibleRange()
        {
            var scrolledY = _content.anchoredPosition.y;
            var viewportHeight = _scrollRect.viewport.rect.height;

            var viewTop = scrolledY - _buffer;
            var viewBottom = scrolledY + viewportHeight + _buffer;

            var rowHeight = _cellHeight + SpacingY;
            var startRow = Mathf.FloorToInt(viewTop / rowHeight);
            var endRow = Mathf.CeilToInt(viewBottom / rowHeight);

            var startIdx = startRow * _constraintCount;
            var endIdx = (endRow + 1) * _constraintCount - 1;

            return (Mathf.Max(0, startIdx), Mathf.Min(_totalItemsCount - 1, endIdx));
        }
        
        public void CreatePlaceholders(int count)
        {
            _totalItemsCount = count;
            UpdateContentSize(count);
            ClearAllSlots();
        }

        public void ShowErrorInSlot(int index, Action action)
        {
            if (_activeItems[index] != null)
            {
                _activeItems[index].ShowError(action);
            }
        }

        public bool IsSlotBusy(int index)
        {
            return _activePlaceholders.ContainsKey(index);
        }

        public void SetPhotoToSlot(int index, Sprite sprite, bool isPremium)
        {
            if (!_activeItems.TryGetValue(index, out var item))
            {
                PrepareSlotForLoading(index);
                item = _activeItems[index];
            }
            item.SetData(index, sprite, isPremium);
        }

        public void SetData(int index, bool isPremium)
        {
            if (!_activeItems.TryGetValue(index, out var item))
            {
                PrepareSlotForLoading(index);
                item = _activeItems[index];
            }

            item.SetData(index, null, isPremium);
        }

        public void PrepareSlotForLoading(int index)
        {
            if (_activeItems.ContainsKey(index))
                return;

            if (!_activePlaceholders.TryGetValue(index, out var placeholder))
            {
                PrepareSlot(index);
                placeholder = _activePlaceholders[index];
            }

            var obj = _pool.Get(placeholder); 
            var photoItem = obj.GetComponent<PhotoItem>();
            photoItem.Reset();
            
            _activeItems[index] = photoItem;
        }

        public void ClearSlot(int index)
        {
            if (_activeItems.Remove(index, out var item))
            {
                _pool.Return(item.gameObject);
            }

            if (_activePlaceholders.Remove(index, out var placeholder))
            {
                _pool.ReturnPlaceholder(placeholder);
            }
        }

        public void ResetScrollPosition()
        {
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void OnDestroy()
        {
            if (_pool == null)
                return;

            foreach (var item in _activeItems.Values)
            {
                if (item != null)
                    _pool.Return(item.gameObject);
            }
        
            foreach (var placeholder in _activePlaceholders.Values)
            {
                if (placeholder != null)
                    _pool.ReturnPlaceholder(placeholder);
            }
        
            _activeItems.Clear();
            _activePlaceholders.Clear();
        }
    }