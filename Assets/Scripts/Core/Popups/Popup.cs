using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField] private Button CloseButton;

    protected virtual void Awake()
    {
        if (CloseButton != null)
            CloseButton.onClick.AddListener(OnClose);
        else
            Debug.LogWarning($"Popup: CloseButton is not assigned on '{gameObject.name}'", this);
    }

    public virtual void Close()
    {
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
        if (CloseButton != null)
            CloseButton.onClick.RemoveListener(OnClose);
    }
}