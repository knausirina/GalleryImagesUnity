using System;
using UnityEngine;

public class Pagination : MonoBehaviour
{
    [SerializeField] private Sprite _activeSprite;
    [SerializeField] private Sprite _inactiveSprite;

    private StateItem[] _buttons;

    public Action<int> OnPageSelect;

    private void Awake()
    {
        _buttons = GetComponentsInChildren<StateItem>();
    }

    private void Start()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            var button = _buttons[i];
            button.SetSprites(_inactiveSprite, _activeSprite);
            int index = i;
            button.Button.onClick.AddListener(() => OnPageSelect?.Invoke(index));
        }
    }

    public void SetActivePage(int visualIndex)
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttons[i].SetActive(i == visualIndex);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttons[i].Button.onClick.RemoveAllListeners();
        }
    }
}