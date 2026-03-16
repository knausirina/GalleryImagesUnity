using UnityEngine;

public class PremiumPrices : MonoBehaviour
{
    [SerializeField] private int _defaultButtonIndex = 0;

    private PriceButton[] _buttons;
    private PriceButton _currentButton;

    private void Awake()
    {
        _buttons = GetComponentsInChildren<PriceButton>();

        foreach (var button in _buttons)
        {
            button.SetState(false);
        }
        OnClick(_buttons[_defaultButtonIndex]);
    }

    private void OnEnable()
    {
        foreach (var button in _buttons)
        {
            var button2 = button;

            button.Button.onClick.AddListener(() => OnClick(button2));
        }
    }

    private void OnDisable()
    {
        foreach (var tab in _buttons)
        {
            tab.Button.onClick.RemoveAllListeners();
        }
    }

    private void OnClick(PriceButton button)
    {
        if (_currentButton == button)
            return;
        _currentButton?.SetState(false);
        _currentButton = button;
        _currentButton.SetState(true);
    }
}