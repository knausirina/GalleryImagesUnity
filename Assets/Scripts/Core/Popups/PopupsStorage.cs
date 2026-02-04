using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PopupsStorage
{
    private DiContainer _diContainer;
    private readonly PopupsConfig _popupsConfig;
    private readonly GameObject _popupsRoot;

    private readonly Dictionary<Type, Popup> _activePopups = new ();

    public PopupsStorage(DiContainer diContainer, PopupsConfig popupsConfig, GameObject popupsRoot)
    {
        _diContainer = diContainer;
        _popupsConfig = popupsConfig;
        _popupsRoot = popupsRoot;
    }

    public T GetView<T>() where T: Popup
    {
        Type type = typeof(T);
        if (_activePopups.TryGetValue(type, out var existingPopup))
        {
            existingPopup.transform.SetAsLastSibling();
            return (T)existingPopup;
        }

        var prefab = _popupsConfig.GetPrefab<T>();
        if (prefab == null)
            throw new InvalidOperationException($"PopupsStorage: No prefab registered for popup type {type.Name}. Check PopupsConfig.");
        if (_popupsRoot == null)
            throw new InvalidOperationException("PopupsStorage: popupsRoot is null. Provide a valid root GameObject.");
        var viewGameObject = _diContainer.InstantiatePrefab(prefab, _popupsRoot.transform);
        if (viewGameObject == null)
            throw new Exception($"PopupsStorage: Failed to instantiate prefab for {type.Name}");
        viewGameObject.transform.localScale = Vector3.one;
        var rectTransform = viewGameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        var component = viewGameObject.GetComponent<T>();
        if (component == null)
        {
            UnityEngine.Object.Destroy(viewGameObject);
            throw new InvalidOperationException($"PopupsStorage: Instantiated prefab for {type.Name} does not contain component {type.Name}.");
        }

        _activePopups.Add(type, component);
        return component;
    }

    public void CloseAll()
    {
        var snapshot = new List<Popup>(_activePopups.Values);
        foreach (var popup in snapshot)
        {
            try
            {
                popup.Close();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }
    }
}