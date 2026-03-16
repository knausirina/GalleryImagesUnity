using System;
using UnityEngine;

public interface IGalleryView
{
    event Action OnScrollChanged;
    void CreatePlaceholders(int count);
    bool IsSlotBusy(int index);
    void SetPhotoToSlot(int index, Sprite sprite, bool isPremium);
    void SetData(int index, bool isPremium);
    void ClearSlot(int index);
    void PrepareSlotForLoading(int index);
    void ResetScrollPosition();
    void ShowErrorInSlot(int index, Action action);
    void ClearAllSlots();
    (int start, int end) GetVisibleRange();
    public void ReleaseSlot(int index);
    public void PrepareSlot(int index);
}