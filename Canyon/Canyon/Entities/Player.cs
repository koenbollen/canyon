using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Canyon.CameraSystem;


namespace Canyon.Entities
{
    public class Player : DrawableGameComponent, IEntity
    {
        public Vector3 Position { get; protected set; }
        public Matrix World { get; protected set; }

        private float yaw;
        protected float Yaw
        {
            get { return yaw; }
            set { yaw = value % MathHelper.TwoPi; orientationChanged = true; }
        }
        private float pitch;
        protected float Pitch
        {
            get { return pitch; }
            set { pitch = value % MathHelper.TwoPi; orientationChanged = true; }
        }
        private float roll;
        protected float Roll
        {
            get { return roll; }
            set { roll = value % MathHelper.TwoPi; orientationChanged = true; }
        }

        private bool orientationChanged;
        public Quaternion Orientation { get; protected set; }
        
        public Vector3 Forward { get { return Vector3.Transform(Vector3.Forward, this.Orientation); } }
        public Vector3 Left { get { return Vector3.Transform(Vector3.Left, this.Orientation); } }
        public Vector3 Up { get { return Vector3.Transform(Vector3.Up, this.Orientation); } }

        private Model model;

        private IFollowCamera followCamera;

        public Player(Game game, Vector3 position)
            : base(game)
        {
            this.Position = position;
        }

        public override void Initialize()
        {
            yaw = -MathHelper.Pi / 4 * 3;

            followCamera = new SimpleFollowCamera(25, -MathHelper.Pi/8);
            CanyonGame.Instance.ChangeCamera(followCamera);

            this.orientationChanged = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.model = Game.Content.Load<Model>("Models/ship");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            World = Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(this.Position);

            Position += this.Forward * 10 * dt;

            if (gameTime.TotalGameTime.TotalSeconds > 2)
                Yaw += MathHelper.Pi / 10 * dt;
            if (gameTime.TotalGameTime.TotalSeconds > 1.9f && gameTime.TotalGameTime.TotalSeconds < 3f)
                Roll += MathHelper.Pi / 8 * dt;
            
            if (this.orientationChanged)
            {
                this.Orientation = Quaternion.CreateFromYawPitchRoll(this.yaw, this.pitch, this.roll);
                this.orientationChanged = false;
            }

            followCamera.Target = this.Position;
            followCamera.Direction = this.Forward;

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
