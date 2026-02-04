using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Configs/Config")]
public class Config : ScriptableObject
{
    [field: SerializeField] public string BaseUrl { get; private set; } = "http://data.ikppbb.com/test-task-unity-data/pics/";
    [field: SerializeField] public int InitialPhotoPoolCapacity { get; private set; } = 10;
    [field: SerializeField] public int TotalImages { get; private set; } = 66;

    public string GetUrlImage(int index)
    {
        if (TotalImages > 0)
        {
            if (index < 0 || index >= TotalImages)
            {
                Debug.LogWarning($"GetUrlImage: index {index} out of range, clamping to valid range.");
                index = Mathf.Clamp(index, 0, TotalImages - 1);
            }
        }

        return $"{BaseUrl}{index + 1}.jpg";
    }
}