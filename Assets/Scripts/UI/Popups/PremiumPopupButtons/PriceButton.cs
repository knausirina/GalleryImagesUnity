using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceButton : MonoBehaviour
{
    [SerializeField] private GameObject _activeObject;
    [field: SerializeField] public Button Button { get; private set; }

    private void Awake()
    {
        _activeObject.SetActive(false);
    }

    public void SetState(bool isActive)
    {
        if (_activeObject != null)
        {
            _activeObject.SetActive(isActive);
        }
    }
}