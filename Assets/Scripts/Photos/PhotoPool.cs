using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotoPool : IDisposable
{
    private readonly GameObject _prefab;
    private readonly GameObject _placePrefab;
    
    private readonly Stack<GameObject> _photoStack = new();
    private readonly Stack<RectTransform> _placeholderStack = new();
    private readonly Transform _poolRoot;
    private readonly DiContainer _diContainer;

    public PhotoPool(DiContainer diContainer,GameObject placePrefab, GameObject prefab, int initialCapacity)
    {
        _diContainer = diContainer;
        _prefab = prefab;
        _placePrefab = placePrefab;
        _poolRoot = new GameObject("[PhotoPool]").transform;
        _poolRoot.gameObject.SetActive(false);

        for (var i = 0; i < initialCapacity; i++)
        {
            _photoStack.Push(CreateNewPhoto());
            _placeholderStack.Push(CreateNewPlaceholder());
        }
    }
    
    private GameObject CreateNewPhoto()
    {
        var obj = _diContainer.InstantiatePrefab(_prefab, _poolRoot);
        obj.SetActive(false);
        return obj;
    }

    public GameObject Get(Transform parent)
    {
        var photoItem = _photoStack.Count > 0 ? _photoStack.Pop() : _diContainer.InstantiatePrefab(_prefab, _poolRoot);
        photoItem.transform.SetParent(parent, false);
        photoItem.transform.localScale = Vector3.one;
        photoItem.SetActive(true);
        return photoItem;
    }

    public void Return(GameObject obj)
    {
        if (obj == null)
            return;
        obj.SetActive(false);
        obj.transform.SetParent(_poolRoot, false);
        _photoStack.Push(obj);
    }
    
    private RectTransform CreateNewPlaceholder()
    {
        var go = _diContainer.InstantiatePrefab(_placePrefab, _poolRoot);
        go.transform.SetParent(_poolRoot, false);
        return go.GetComponent<RectTransform>();
    }
    
    public RectTransform GetPlaceholder()
    {
        var rt = _placeholderStack.Count > 0 ? _placeholderStack.Pop() : CreateNewPlaceholder();
        rt.gameObject.SetActive(true);
        return rt;
    }
    
    public void ReturnPlaceholder(RectTransform rt)
    {
        if (rt == null) return;
        rt.gameObject.SetActive(false);
        rt.SetParent(_poolRoot, false);
        _placeholderStack.Push(rt);
    }

    public void Dispose()
    {
        while (_photoStack.Count > 0)
        {
            var obj = _photoStack.Pop();
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }
        
        while (_placeholderStack.Count > 0)
        {
            var obj = _placeholderStack.Pop();
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }

        if (_poolRoot != null && _poolRoot.gameObject != null)
            UnityEngine.Object.Destroy(_poolRoot.gameObject);
    }
}