using UnityEngine;
using UnityEngine.UI;

public class BannerPageView : MonoBehaviour
{
    private Sprite _active;
    private Sprite _inactive;

    private Image _image;

    public Button Button { get; private set; }

    public void SetSprites(Sprite inactiveSprite, Sprite activeSprite)
    {
        _inactive = inactiveSprite;
        _active = activeSprite;
    }

    public void Awake()
    {
        _image = GetComponent<Image>();
        Button = GetComponent<Button>();
    }

    public void SetActive(bool isActive)
    {
        _image.sprite = isActive ? _active : _inactive;
    }
}