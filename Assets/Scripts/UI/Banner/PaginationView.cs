using System;
using UnityEngine;

public class PaginationView : MonoBehaviour
{
    [SerializeField] private Sprite _activeSprite;
    [SerializeField] private Sprite _inactiveSprite;

    private BannerPageView[] _buttons;

    public Action<int> OnPageSelectAction;

    private void Awake()
    {
        _buttons = GetComponentsInChildren<BannerPageView>();
    }

    private void Start()
    {
        for (var i = 0; i < _buttons.Length; i++)
        {
            var button = _buttons[i];
            button.SetSprites(_inactiveSprite, _activeSprite);
            var index = i;
            button.Button.onClick.AddListener(() => OnPageSelectAction?.Invoke(index));
        }
    }

    public void SetActivePage(int visualIndex)
    {
        for (var i = 0; i < _buttons.Length; i++)
        {
            _buttons[i].SetActive(i == visualIndex);
        }
    }

    private void OnDestroy()
    {
        for (var i = 0; i < _buttons.Length; i++)
        {
            _buttons[i].Button.onClick.RemoveAllListeners();
        }
    }
}