namespace Assets.Project.Code.Runtime.Model
{
    public sealed class CubeData
    {
        public string ColorId { get; }
        public float PositionX { get; }
        public float Size { get; }

        public CubeData(string colorId, float positionX, float size)
        {
            ColorId = colorId;
            PositionX = positionX;
            Size = size;
        }
    }
}
