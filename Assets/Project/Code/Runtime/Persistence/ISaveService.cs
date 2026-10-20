namespace Assets.Project.Code.Runtime.Persistence
{
    public interface ISaveService
    {
        bool TryLoad(out GameSaveData data);
        void Save(GameSaveData data);
    }
}
