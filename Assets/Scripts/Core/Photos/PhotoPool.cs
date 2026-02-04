using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotoPool : IDisposable
{
    private readonly GameObject _prefab;
    private readonly Stack<GameObject> _stack = new();
    private readonly Transform _poolRoot;
    private readonly DiContainer _diContainer;

    public PhotoPool(DiContainer diContainer, GameObject prefab, int initialCapacity)
    {
        _diContainer = diContainer;
        _prefab = prefab;
        _poolRoot = new GameObject("[PhotoPool]").transform;
        _poolRoot.gameObject.SetActive(false);

        for (int i = 0; i < initialCapacity; i++)
        {
            var obj = CreateNew();
            _stack.Push(obj);
        }
    }

    private GameObject CreateNew()
    {
        var obj = _diContainer.InstantiatePrefab(_prefab, _poolRoot);
        obj.SetActive(false);
        return obj;
    }

    public GameObject Get(Transform parent)
    {
        GameObject photoItem = _stack.Count > 0 ? _stack.Pop() : _diContainer.InstantiatePrefab(_prefab, _poolRoot);

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
        _stack.Push(obj);
    }

    public void Dispose()
    {
        while (_stack.Count > 0)
        {
            var obj = _stack.Pop();
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }

        if (_poolRoot != null && _poolRoot.gameObject != null)
            UnityEngine.Object.Destroy(_poolRoot.gameObject);
    }
}