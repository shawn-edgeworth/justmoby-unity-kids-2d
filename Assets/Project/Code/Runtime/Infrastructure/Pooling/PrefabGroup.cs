using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Code.Runtime.Infrastructure.Pooling
{
    internal sealed class PrefabGroup
    {
        private readonly Transform parent;
        private readonly Queue<GameObject> pool = new();

        public int PrefabKey { get; }
        public GameObject Prefab { get; }

        public PrefabGroup(int prefabKey, GameObject prefab, Transform parent)
        {
            PrefabKey = prefabKey;
            Prefab = prefab;
            this.parent = parent;
        }

        public bool TryGet(out GameObject instance)
        {
            if (pool.Count == 0)
            {
                instance = null;
                return false;
            }

            instance = pool.Dequeue();
            return true;
        }

        public void Return(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localEulerAngles = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            pool.Enqueue(instance);
        }
    }
}
