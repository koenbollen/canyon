using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Canyon.Entities
{
    public class Player : DrawableGameComponent, IEntity
    {
        public Vector3 Position { get; protected set; }
        public Matrix World { get; protected set; }

        private float yaw;
        private float pitch;
        private float roll;

        private bool orientationChanged;
        public Quaternion Orientation { get; protected set; }
        
        public Vector3 Forward { get { return Vector3.Transform(Vector3.Forward, this.Orientation); } }
        public Vector3 Left { get { return Vector3.Transform(Vector3.Left, this.Orientation); } }
        public Vector3 Up { get { return Vector3.Transform(Vector3.Up, this.Orientation); } }

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
            World = Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(this.Position);
            
            // Demo rotation:
            this.yaw += MathHelper.Pi / 8 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.orientationChanged = true;



            if (this.orientationChanged)
            {
                this.Orientation = Quaternion.CreateFromYawPitchRoll(this.yaw, this.pitch, this.roll);
                this.orientationChanged = false;
            }

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
                    effect.PreferPerPixelLighting = true;
                    effect.DiffuseColor = Color.DarkMagenta.ToVector3();
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }

    }
}
