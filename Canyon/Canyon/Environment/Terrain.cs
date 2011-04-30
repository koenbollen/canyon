using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Canyon.Misc;


namespace Canyon.Environment
{
    public class Terrain : DrawableGameComponent
    {
        /// <summary>
        /// The Vertex information specific for this terrain.
        /// </summary>
        private struct VertexTerrain : IVertexType
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector3 Color;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Color, 0)
            );

            VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        }

        private struct TerrainTile
        {
            public VertexBuffer Buffer;
            public BoundingBox BoundingBox;

            public TerrainTile(VertexBuffer buffer, BoundingBox boundingBox)
            {
                this.Buffer = buffer;
                this.BoundingBox = boundingBox;
            }
        }

        public int TileSize { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        private float maxheight;
        private float minheight;

        private string heightmapAsset;
        private Texture2D heightmap;
        private float[,] data;

        private ContentManager content;
        private Effect effect;
        private IndexBuffer index;
        private TerrainTile[] tiles;

        private BoundingFrustum frustum;
        private int[] indices;

        public Terrain(Game game, string heightmap)
            : base(game)
        {
            this.heightmapAsset = heightmap;
        }

        public override void Initialize()
        {
            TileSize = 32;
            CanyonGame.Camera.CameraChanged += new CameraSystem.OnCameraChanged(CameraChanged);
            base.Initialize();
            CanyonGame.Console.Trace("Terrain initialized.");
        }

        void CameraChanged(Matrix view, Matrix projection)
        {
            if( !Keyboard.GetState().IsKeyDown(Keys.LeftAlt) )
                this.frustum = new BoundingFrustum(view * projection);
        }

        protected override void LoadContent()
        {
            this.content = new ContentManager(Game.Services);
            this.content.RootDirectory = "Content";
            this.effect = content.Load<Effect>("FX/terrain");
            this.heightmap = content.Load<Texture2D>(this.heightmapAsset);

            CanyonGame.Console.Debug("Loading heightmap of " + this.heightmap.Width + "x" + this.heightmap.Height + " (TileSize is " + TileSize + ").");

            this.LoadData(); // Width and Height are available from here on out.
            this.LoadIndices();
            this.LoadVertices();

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
        /// Create and IndexBuffer with the indices of one tile.
        /// </summary>
        private void LoadIndices()
        {
            int size = TileSize;
            indices = new int[size * size * 6];
            int c = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int lowerLeft = x + y * (size+1);
                    int lowerRight = (x + 1) + y * (size + 1);
                    int topLeft = x + (y + 1) * (size + 1);
                    int topRight = (x + 1) + (y + 1) * (size + 1);

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

        /// <summary>
        /// Construct the vertices and the vertexbuffer.
        /// </summary>
        private void LoadVertices()
        {
            TileSize = Math.Min(Math.Min(this.Width, this.Height), TileSize);

            int widthInTiles = (int)Math.Ceiling((this.Width - 1) / (double)(TileSize));
            int heightInTiles = (int)Math.Ceiling((this.Height - 1) / (double)(TileSize));

            this.tiles = new TerrainTile[widthInTiles * heightInTiles];
            Vector3 color = new Vector3(0xed/255.0f, 0xaa/255.0f, 0x09/255.0f);
            for (int ty = 0; ty < heightInTiles; ty++)
            {
                for (int tx = 0; tx < widthInTiles; tx++)
                {
                    int width = TileSize + 1;
                    int height = TileSize + 1;

                    VertexTerrain[] vertices = new VertexTerrain[width * height];

                    Vector3 bbmax = Vector3.One * float.MinValue;
                    Vector3 bbmin = Vector3.One * float.MaxValue;
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int rx = (tx * TileSize + x);
                            int ry = (ty * TileSize + y);
                            int ri = rx + ry * this.Width;
                            int i = x + y * width;
                            vertices[i].Position = new Vector3(rx, data[Math.Min(rx, this.Width - 1), Math.Min(ry, this.Height - 1)], ry);
                            bbmax = Vector3.Max(bbmax, vertices[i].Position);
                            bbmin = Vector3.Min(bbmin, vertices[i].Position);

                            float weight = vertices[i].Position.Y;
                            weight -= minheight;
                            weight /= (maxheight - minheight);
                            weight *= weight;
                            vertices[i].Color = Vector3.Lerp(color, Vector3.One, weight); 
                        }
                    }

                    this.SetupNormals(vertices);

                    VertexBuffer buffer = new VertexBuffer(GraphicsDevice, typeof(VertexTerrain), vertices.Length, BufferUsage.None);
                    buffer.SetData(vertices);

                    tiles[ty * widthInTiles + tx] = new TerrainTile(buffer, new BoundingBox(bbmin, bbmax));
                }
            }
        }

        private void SetupNormals(VertexTerrain[] vertices)
        {
            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index0 = indices[i * 3];
                int index1 = indices[i * 3 + 1];
                int index2 = indices[i * 3 + 2];

                Vector3 side0 = vertices[index0].Position - vertices[index2].Position;
                Vector3 side1 = vertices[index0].Position - vertices[index1].Position;
                Vector3 normal = Vector3.Cross(side1, side0);

                vertices[index0].Normal += normal;
                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = vertices[i].Normal.SafeNormalize();
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


            effect.Parameters["LightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));
            effect.Parameters["Ambient"].SetValue(0.2f);

            effect.CurrentTechnique.Passes[0].Apply();

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.Indices = index;

            foreach (TerrainTile tt in this.tiles)
            {
                if (!frustum.Intersects(tt.BoundingBox))
                    continue;
                GraphicsDevice.SetVertexBuffer(tt.Buffer);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, tt.Buffer.VertexCount, 0, index.IndexCount / 3 );
            }
            
            base.Draw(gameTime);
        }
    }
}
