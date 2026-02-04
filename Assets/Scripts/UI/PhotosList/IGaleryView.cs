using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

public interface IGalleryView
{
    event Action OnScrollChanged;
    void CreatePlaceholders(int count);
    void ToggleSlot(int index, bool active);
    bool IsVisible(int index);
    bool IsSlotBusy(int index);
    void SetPhotoToSlot(int index, Sprite sprite, bool isPremium);
    void SetData(int index, bool isPremium);
    void ClearSlot(int index);
    void PrepareSlotForLoading(int index);
    void ResetScrollPosition();
    UniTask RefreshLayoutAsync(CancellationToken token);
    UniTask FadeAsync(float alpha, float duration, CancellationToken token);
    void ShowErrorInSlot(int index, Action action);
}