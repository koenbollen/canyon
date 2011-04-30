using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Canyon.Entities
{
    public class Player : DrawableGameComponent, IEntity
    {
        public Vector3 Position { get; protected set; }
        public Matrix World { get; protected set; }

        private Model model;

        public Player(Game game, Vector3 position)
            : base(game)
        {
            this.Position = position;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.model = Game.Content.Load<Model>("Models/ship");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            World = Matrix.CreateTranslation(this.Position);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix[] transforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * this.World;
                    effect.View = CanyonGame.Camera.View;
                    effect.Projection = CanyonGame.Camera.Projection;
                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }

    }
}
