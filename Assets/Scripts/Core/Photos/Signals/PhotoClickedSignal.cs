public class PhotoClickedSignal
{
    public int Index { get; private set; }
    public bool IsPremium { get; private set; }
    public PhotoClickedSignal(int index, bool isPremium)
    {
        Index = index;
        IsPremium = isPremium;
    }
}