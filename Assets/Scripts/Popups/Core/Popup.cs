using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    
    public event System.Action<Popup> OnClosed;

    protected virtual void Awake()
    {
        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClose);
        else
            Debug.LogWarning($"Popup: CloseButton is not assigned on '{gameObject.name}'", this);
    }

    public virtual void Close()
    {
        OnClosed?.Invoke(this);
        gameObject.SetActive(false);
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    private void OnClose()
    {
        Close();
    }

    protected virtual void OnDestroy()
    {
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(OnClose);
    }
}