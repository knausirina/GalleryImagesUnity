public class FilterChangedSignal
{
    public GalleryType Type { get; }

    public FilterChangedSignal(GalleryType type)
    {
        Type = type;
    }
}