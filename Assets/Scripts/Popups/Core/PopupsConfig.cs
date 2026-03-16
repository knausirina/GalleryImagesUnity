using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PopupsConfig", menuName = "Configs/PopupsConfig")]
public class PopupsConfig : ScriptableObject
{
    [SerializeField] private List<GameObject> _popupsPrefabs;

    private Dictionary<Type, GameObject> _cache;

    private void Initialize()
    {
        _cache = new Dictionary<Type, GameObject>();

        if (_popupsPrefabs == null)
            return;

        foreach (var prefab in _popupsPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[PopupsConfig] Null prefab in _popupsPrefabs list");
                continue;
            }

            var components = prefab.GetComponentsInChildren<Popup>(true);
            foreach (var comp in components)
            {
                if (comp == null) continue;

                Type type = comp.GetType();
                if (!_cache.TryAdd(type, prefab))
                {
                    Debug.LogWarning($"[PopupsConfig] Duplicate type {type.Name} on prefab {prefab.name}");
                }
            }
        }
    }

    public GameObject GetPrefab<T>() where T : Popup
    {
        if (_cache == null)
            Initialize();

        if (_cache.TryGetValue(typeof(T), out var prefab))
        {
            return prefab;
        }

        return null;
    }
}