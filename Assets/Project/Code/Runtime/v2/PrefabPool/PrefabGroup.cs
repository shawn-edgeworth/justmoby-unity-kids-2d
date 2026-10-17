using System.Collections.Generic;
using UnityEngine;

internal class PrefabGroup
{
    public readonly int PrefabKey;
    public readonly GameObject Prefab;
    private readonly Transform parent;
    private readonly Queue<GameObject> pool = new();

    public int Count => pool.Count;

    public PrefabGroup(int prefabKey, GameObject prefab, Transform parent)
    {
        PrefabKey = prefabKey;
        Prefab = prefab;
        this.parent = parent;
    }

    public bool TryGet(out GameObject gameObject)
    {
        gameObject = pool.Count > 0 ? pool.Dequeue() : null;
        return gameObject != null;
    }

    public void Return(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(parent);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localEulerAngles = Vector3.zero;
        pool.Enqueue(gameObject);
    }
}
