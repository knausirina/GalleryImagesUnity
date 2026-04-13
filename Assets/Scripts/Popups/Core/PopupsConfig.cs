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

            var component = prefab.GetComponent<Popup>();
            if (component != null)
                _cache[component.GetType()] = prefab;
        }
    }

    public GameObject GetPrefab<T>() where T : Popup
    {
        if (_cache == null)
            Initialize();

        return _cache.GetValueOrDefault(typeof(T));
    }
}