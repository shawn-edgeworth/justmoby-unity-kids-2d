using UnityEngine;

namespace Assets.Project.Code.Runtime.Infrastructure.Pooling
{
    public interface IPrefabPool
    {
        GameObject Spawn(GameObject prefab);
        void Despawn(GameObject instance);
    }
}
