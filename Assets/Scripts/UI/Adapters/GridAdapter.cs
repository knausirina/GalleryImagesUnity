using UnityEngine;
using UnityEngine.UI;

public class GridAdapter : MonoBehaviour
{
    [SerializeField] private float _spacing = 20f;
    [SerializeField] private float _paddingSide = 30f;
    
    private GridLayoutGroup _grid;
    private RectTransform _rectTransform;

    private void Start()
    {
        _grid = GetComponent<GridLayoutGroup>();
        _rectTransform = GetComponent<RectTransform>();

        AdjustGrid();
    }

    public void AdjustGrid()
    {
        int columns = DetecterTablet.IsTablet() ? 3 : 2;
        _grid.constraintCount = columns;

        float parentWidth = _rectTransform.rect.width;
        if (parentWidth <= 0)
            return;


        _grid.padding.left = (int)_paddingSide;
        _grid.padding.right = (int)_paddingSide;
        _grid.spacing = new Vector2(_spacing, _spacing);

        float totalPaddings = _grid.padding.left + _grid.padding.right;
        float totalSpacings = _grid.spacing.x * (columns - 1);
        float availableWidth = parentWidth - totalPaddings - totalSpacings;

        float cellSize = availableWidth / columns;

        _grid.cellSize = new Vector2(cellSize - 10, cellSize - 10);
    }
}