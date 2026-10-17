using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.v2.PrefabPool
{
    public class PrefabPool : IPrefabPool, IInitializable
    {
        private readonly Dictionary<int, PrefabGroup> prefabGroups = new();

        [Inject] private DiContainer diContainer;

        private Transform PoolRoot { get; set; }

        public void Initialize()
        {
            PoolRoot = new GameObject("PrefabPool").transform;
            UnityEngine.Object.DontDestroyOnLoad(PoolRoot.gameObject);
        }

        public GameObject Spawn(GameObject prefab)
        {
            var prefabKey = GetPrebabKey(prefab);

            if (!prefabGroups.TryGetValue(prefabKey, out var prefabGroup))
            {
                var prefabGroupParent = new GameObject($"Group_{GetPrefabName(prefab)}");
                prefabGroupParent.transform.SetParent(PoolRoot);

                prefabGroup = new PrefabGroup(prefabKey, prefab, prefabGroupParent.transform);
                prefabGroups.Add(prefabKey, prefabGroup);
            }

            if (!prefabGroup.TryGet(out var instance))
            {
                instance = diContainer.InstantiatePrefab(prefab);
                instance.name = GetPrefabName(prefab);
                var poolableObject = instance.AddComponent<PoolableObjectInfo>();
                poolableObject.Initialize(prefabKey);

            }

            foreach (var poolableObject in instance.GetComponentsInChildren<IPoolableObject>())
            {
                poolableObject.OnSpawn();
            }
            instance.SetActive(true);
            instance.transform.SetParent(null);

            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!instance.TryGetComponent<PoolableObjectInfo>(out var poolableObjectInfo))
            {
                return;
            }

            if (!prefabGroups.TryGetValue(poolableObjectInfo.PrefabKey, out var prefabGroup))
            {
                return;
            }

            foreach (var poolableObject in instance.GetComponentsInChildren<IPoolableObject>())
            {
                poolableObject.OnDespawn();
            }

            prefabGroup.Return(instance);
        }

        private string GetPrefabName(GameObject prefab)
        {
            return $"{prefab.name}_{GetPrebabKey(prefab)}";
        }
        private int GetPrebabKey(GameObject prefab)
        {
            return prefab.GetInstanceID();
        }
    }
}