using UnityEngine;

public class CustomHeight : MonoBehaviour
{
    [SerializeField] private RectTransform _rect;
    [SerializeField] private int _offset;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        float canvasHeight = canvasRect.rect.height;

        _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, (canvasHeight - _offset) < 1500 ? 1500 :  (canvasHeight - _offset));
    }

    [ContextMenu("1")]
    private void Update1()
    {
        Awake();
    }
}
