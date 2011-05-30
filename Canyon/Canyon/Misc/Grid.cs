using Canyon.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Canyon.Misc
{
    public class Grid : DrawableGameComponent
    {
        public IEntity Follow { get; set; }
        private int size;
        private float gridstep;

        private BasicEffect effect;
        private VertexPositionColor[] vertices;
        private VertexBuffer buffer;
        private Matrix world;

        public Grid(Game game, int size, float gridstep, IEntity follow)
            : base(game)
        {
            this.size = size;
            this.gridstep = gridstep;
            this.Follow = follow;
        }

        public override void Initialize()
        {
            world = Matrix.CreateTranslation(new Vector3(-.5f, 0, -.5f) * size * gridstep);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = new BasicEffect(GraphicsDevice);
            vertices = new VertexPositionColor[size * 4];
            int i = 0;
            for (int x = 0; x < size; x++)
            {
                vertices[i++] = new VertexPositionColor(new Vector3(x * gridstep, 0, 0), Color.White);
                vertices[i++] = new VertexPositionColor(new Vector3(x * gridstep, 0, (size - 1)*gridstep), Color.White);
            }
            for (int y = 0; y < size; y++)
            {
                vertices[i++] = new VertexPositionColor(new Vector3(0, 0, y * gridstep), Color.White);
                vertices[i++] = new VertexPositionColor(new Vector3((size - 1) * gridstep, 0, y * gridstep), Color.White);
            }
            buffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), i, BufferUsage.None);
            buffer.SetData(vertices);
            base.LoadContent();
        }


        public override void Draw(GameTime gameTime)
        {
            if (Follow != null)
            {
                Vector3 position = new Vector3(Follow.Position.X - (Follow.Position.X % gridstep), 0, Follow.Position.Z - (Follow.Position.Z % gridstep));
                world = Matrix.CreateTranslation(position + (new Vector3(-.5f, 0, -.5f) * size * gridstep));
            }
            effect.World = world;
            effect.View = CanyonGame.Camera.View;
            effect.Projection = CanyonGame.Camera.Projection;

            effect.Alpha = .1f;
            effect.VertexColorEnabled = true;

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SetVertexBuffer(buffer);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, size * 4);
            }
            base.Draw(gameTime);
        }
    }
}
