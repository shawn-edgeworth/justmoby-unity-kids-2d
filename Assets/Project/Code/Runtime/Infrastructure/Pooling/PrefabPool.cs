using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Assets.Project.Code.Runtime.Infrastructure.Pooling
{
    public sealed class PrefabPool : IPrefabPool, IInitializable
    {
        private readonly Dictionary<int, PrefabGroup> prefabGroups = new();
        private readonly DiContainer diContainer;

        private Transform poolRoot;

        public PrefabPool(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }

        public void Initialize()
        {
            poolRoot = new GameObject("V4_PrefabPool").transform;
            UnityEngine.Object.DontDestroyOnLoad(poolRoot.gameObject);
        }

        public GameObject Spawn(GameObject prefab)
        {
            var prefabKey = GetPrefabKey(prefab);

            if (!prefabGroups.TryGetValue(prefabKey, out var prefabGroup))
            {
                var groupParent = new GameObject($"Group_{prefab.name}_{prefabKey}");
                groupParent.transform.SetParent(poolRoot);
                prefabGroup = new PrefabGroup(prefabKey, prefab, groupParent.transform);
                prefabGroups.Add(prefabKey, prefabGroup);
            }

            if (!prefabGroup.TryGet(out var instance))
            {
                instance = diContainer.InstantiatePrefab(prefab);
                instance.name = $"{prefab.name}_{prefabKey}";
                var info = instance.AddComponent<PoolableObjectInfo>();
                info.Initialize(prefabKey);
            }

            var poolables = instance.GetComponentsInChildren<IPoolableObject>(true);
            for (var i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnSpawn();
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

            if (!instance.TryGetComponent<PoolableObjectInfo>(out var info))
            {
                throw new InvalidOperationException(
                    $"Cannot despawn '{instance.name}': missing {nameof(PoolableObjectInfo)}.");
            }

            if (!prefabGroups.TryGetValue(info.PrefabKey, out var prefabGroup))
            {
                throw new InvalidOperationException(
                    $"Cannot despawn '{instance.name}': unknown prefab key {info.PrefabKey}.");
            }

            var poolables = instance.GetComponentsInChildren<IPoolableObject>(true);
            for (var i = 0; i < poolables.Length; i++)
            {
                poolables[i].OnDespawn();
            }

            prefabGroup.Return(instance);
        }

        private static int GetPrefabKey(GameObject prefab)
        {
            return prefab.GetInstanceID();
        }
    }
}
