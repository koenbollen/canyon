using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Environment
{
    public struct TerrainTile
    {
        public Vector2 Position;
        public VertexBuffer Buffer;
        public BoundingBox BoundingBox;

        public TerrainTile(Vector2 position, VertexBuffer buffer, BoundingBox boundingBox)
        {
            this.Position = position;
            this.Buffer = buffer;
            this.BoundingBox = boundingBox;
        }
    }
}
