using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Environment
{
    public struct TerrainTile
    {
        public int Width;
        public int Height;
        public Vector3 Position;
        public float[,] data;
        public VertexBuffer Buffer;
    }
}
