using System.Collections.Generic;

public class GalleryFilterManager
{
    private readonly Config _config;
    private readonly List<int> _filteredIndices = new();
    private GalleryType _currentFilter = GalleryType.All;

    public GalleryFilterManager(Config config)
    {
        _config = config;
        UpdateFilteredIndices();
    }

    public void SetFilter(GalleryType filter)
    {
        _currentFilter = filter;
        UpdateFilteredIndices();
    }

    public IReadOnlyList<int> GetFilteredIndices() => _filteredIndices;

    private void UpdateFilteredIndices()
    {
        _filteredIndices.Clear();
        for (var i = 0; i < _config.TotalImages; i++)
        {
            if (IsMatchFilter(i))
            {
                _filteredIndices.Add(i);
            }
        }
    }

    private bool IsMatchFilter(int index) => _currentFilter switch
    {
        GalleryType.Odd => (index + 1) % 2 != 0,
        GalleryType.Even => (index + 1) % 2 == 0,
        _ => true
    };
}