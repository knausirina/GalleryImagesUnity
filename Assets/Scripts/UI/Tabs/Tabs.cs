using UnityEngine;

public class Tabs : MonoBehaviour
{
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _inactiveColor;

    [SerializeField] private int _defaultTabIndex = 0;

    private TabButton[] _tabs;
    private TabButton _currentActiveTab;

    private void Awake()
    {
        _tabs = GetComponentsInChildren<TabButton>();

        foreach (TabButton tab in _tabs)
        {
            tab.SetColor(_activeColor, _inactiveColor);
            tab.SetState(false);
        }
        OnClickTab(_tabs[_defaultTabIndex]);
    }

    private void OnEnable()
    {
        foreach (TabButton tab in _tabs)
        {
            TabButton tabValue = tab;
            tab.Button.onClick.AddListener(() => OnClickTab(tabValue));
        }
    }

    private void OnDisable()
    {
        foreach (TabButton tab in _tabs)
        {
            tab.Button.onClick.RemoveAllListeners();
        }
    }

    private void OnClickTab(TabButton activeTabButton)
    {
        if (_currentActiveTab == activeTabButton)
            return;

        if (_currentActiveTab != null)
            _currentActiveTab.SetState(false);
        _currentActiveTab = activeTabButton;
        _currentActiveTab.SetState(true);
    }
}