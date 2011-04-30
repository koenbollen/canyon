using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


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

            TileSize = Math.Min(Math.Min(this.Width, this.Height), TileSize);

            int widthInTiles = (int)Math.Ceiling((this.Width-1) / (double)(TileSize));
            int heightInTiles = (int)Math.Ceiling((this.Height-1) / (double)(TileSize));

            this.tiles = new TerrainTile[widthInTiles * heightInTiles];

            for (int ty = 0; ty < heightInTiles; ty++)
            {
                for (int tx = 0; tx < widthInTiles; tx++)
                {
                    Vector2 position = new Vector2(tx, ty);

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
                            vertices[i].Position = new Vector3(rx, data[Math.Min(rx,this.Width-1), Math.Min(ry,this.Height-1)], ry);
                            bbmax = Vector3.Max(bbmax, vertices[i].Position);
                            bbmin = Vector3.Min(bbmin, vertices[i].Position);
                        }
                    }

                    VertexBuffer buffer = new VertexBuffer(GraphicsDevice, typeof(VertexTerrain), vertices.Length, BufferUsage.None);
                    buffer.SetData(vertices);

                    tiles[ty * widthInTiles + tx] = new TerrainTile(position, buffer, new BoundingBox(bbmin, bbmax));
                }
            }
        }

        private void LoadIndices()
        {
            int size = TileSize;
            int[] indices = new int[size * size * 6];
            int c = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int topLeft = x + y * (size+1);
                    int topRight = (x + 1) + y * (size + 1);
                    int lowerLeft = x + (y + 1) * (size + 1);
                    int lowerRight = (x + 1) + (y + 1) * (size + 1);

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
