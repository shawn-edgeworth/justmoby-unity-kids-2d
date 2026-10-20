namespace Assets.Project.Code.Runtime.Infrastructure.Pooling
{
    public interface IPoolableObject
    {
        void OnSpawn();
        void OnDespawn();
    }
}
