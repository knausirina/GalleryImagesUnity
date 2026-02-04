using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TabButton : MonoBehaviour
{
    [SerializeField] private GameObject _activeObject;
    [SerializeField] private GalleryFilter _filterType;
    [SerializeField]  private TMP_Text _text;
    [field: SerializeField] public Button Button { get; private set; }

    private Color _activeColor;
    private Color _inactiveColor;
    private SignalBus _signalBus;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void SetColor(Color activeColor, Color inactiveColor)
    {
        _activeColor = activeColor;
        _inactiveColor = inactiveColor;
    }

    private void Awake()
    {
        if (_activeObject != null)
            _activeObject.SetActive(false);

        Button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _signalBus.Fire(new FilterChangedSignal(_filterType));
    }

    public void SetState(bool isActive)
    {
        if (_activeObject != null)
            _activeObject.SetActive(isActive);

         _text.color = isActive ? _activeColor : _inactiveColor; 
    }

    private void OnDestroy()
    {
        Button.onClick.RemoveListener(OnClick);
    }
}