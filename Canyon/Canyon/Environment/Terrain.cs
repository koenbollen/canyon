using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Canyon.Environment
{
    public class Terrain : DrawableGameComponent
    {
        /// <summary>
        /// The Vertex information specific for this terrain.
        /// </summary>
        struct VertexTerrain : IVertexType
        {
            public Vector3 Position;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
            );

            VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        private float maxheight;
        private float minheight;

        private string heightmapAsset;
        private Texture2D heightmap;
        private float[,] data;

        private Effect effect;
        private ContentManager content;
        private VertexTerrain[] vertices;
        private int[] indices;
        private VertexBuffer buffer;
        private IndexBuffer index;

        public Terrain(Game game, string heightmap)
            : base(game)
        {
            this.heightmapAsset = heightmap;
        }

        public override void Initialize()
        {
            base.Initialize();
            CanyonGame.Console.Trace("Terrain initialized.");
        }

        protected override void LoadContent()
        {
            this.content = new ContentManager(Game.Services);
            this.content.RootDirectory = "Content";
            this.effect = content.Load<Effect>("FX/terrain");
            this.heightmap = content.Load<Texture2D>(this.heightmapAsset);

            this.LoadData(); // Width and Height are available from here on out.
            this.LoadVertices();
            this.LoadIndices();

            base.LoadContent();
        }

        /// <summary>
        /// Convert the Texture2D to float[,] and calculate the height bounds.
        /// </summary>
        private void LoadData()
        {
            this.Width = this.heightmap.Width;
            this.Height = this.heightmap.Height;
            Color[] pixels = new Color[this.Width * this.Height];
            this.heightmap.GetData(pixels);

            maxheight = float.MinValue;
            minheight = float.MaxValue;

            this.data = new float[this.Width, this.Height];
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    data[x, y] = pixels[x + y * this.Width].R / 5f; // Red channel is terrain height.
                    maxheight = Math.Max(maxheight, data[x, y]);
                    minheight = Math.Min(minheight, data[x, y]);
                }
            }
        }

        /// <summary>
        /// Construct the vertices and the vertexbuffer.
        /// </summary>
        private void LoadVertices()
        {
            this.vertices = new VertexTerrain[this.Width * this.Height];
            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    int i = x + y * this.Width;
                    this.vertices[i].Position = new Vector3(x, data[x, y], y);
                }
            }
            this.buffer = new VertexBuffer(GraphicsDevice, typeof(VertexTerrain), this.Width * this.Height, BufferUsage.None);
            this.buffer.SetData(this.vertices);
        }

        private void LoadIndices()
        {
            indices = new int[(this.Width - 1) * (this.Height - 1) * 6];
            int c = 0;
            for (int y = 0; y < this.Height - 1; y++)
            {
                for (int x = 0; x < this.Width - 1; x++)
                {
                    int lowerLeft = x + y * Width;
                    int lowerRight = (x + 1) + y * Width;
                    int topLeft = x + (y + 1) * Width;
                    int topRight = (x + 1) + (y + 1) * Width;

                    indices[c++] = topLeft;
                    indices[c++] = lowerRight;
                    indices[c++] = lowerLeft;

                    indices[c++] = topLeft;
                    indices[c++] = topRight;
                    indices[c++] = lowerRight;
                }
            }

            index = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
            index.SetData(indices);
        }

        protected override void UnloadContent()
        {
            data = null;
            this.content.Unload();
            base.UnloadContent();
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            effect.CurrentTechnique = effect.Techniques["JustWhite"];

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(CanyonGame.Camera.View);
            effect.Parameters["Projection"].SetValue(CanyonGame.Camera.Projection);

            effect.CurrentTechnique.Passes[0].Apply();

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rs;

            GraphicsDevice.SetVertexBuffer(this.buffer);
            GraphicsDevice.Indices = index;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, vertices.Length * 2);

            base.Draw(gameTime);
        }
    }
}
