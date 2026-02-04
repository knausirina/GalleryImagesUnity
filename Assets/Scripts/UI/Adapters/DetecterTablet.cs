using UnityEngine;

public class DetecterTablet
{
    public static bool IsTablet()
    {
        float screenWidth = Mathf.Max(Screen.width, Screen.height);
        float screenHeight = Mathf.Min(Screen.width, Screen.height);
        float aspectRatio = screenWidth / screenHeight;

        float dpi = Screen.dpi > 0 ? Screen.dpi : 160;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(Screen.width, 2) + Mathf.Pow(Screen.height, 2)) / dpi;

        return diagonalInches > 6.5f && aspectRatio < 1.7f;
    }
}