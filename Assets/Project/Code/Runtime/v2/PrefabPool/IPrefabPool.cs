using UnityEngine;

namespace Assets.Project.Code.Runtime.v2.PrefabPool
{
    public interface IPrefabPool
    {
        GameObject Spawn(GameObject prefab);
        void Despawn(GameObject gameObject);
    }
}