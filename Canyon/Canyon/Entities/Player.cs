using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Canyon.CameraSystem;


namespace Canyon.Entities
{
    public class Player : DrawableGameComponent, IEntity
    {
        public const float PitchStep = MathHelper.Pi / 4;
        public const float YawStep = MathHelper.Pi / 4;
        public const float RollStep = MathHelper.PiOver2;

        public Vector3 Position { get; protected set; }
        public Matrix World { get; protected set; }

        public Quaternion Orientation { get; protected set; }
        public Vector3 Forward { get { return Vector3.Transform(Vector3.Forward, this.Orientation); } }
        public Vector3 Left { get { return Vector3.Transform(Vector3.Left, this.Orientation); } }
        public Vector3 Up { get { return Vector3.Transform(Vector3.Up, this.Orientation); } }

        private Model model;

        private IFollowCamera followCamera;

        protected InputManager Input;
        public Player(Game game, Vector3 position)
            : base(game)
        {
            this.Position = position;
        }

        public override void Initialize()
        {
            Input = CanyonGame.Input;
            Input.CenterMouse = true;

            Orientation = Quaternion.Identity;

            followCamera = new SimpleFollowCamera(25, -MathHelper.Pi/8);
            CanyonGame.Instance.ChangeCamera(followCamera);

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

            this.Position += this.Forward * -Input.Movement.Z * 50 * dt; // tmp ofc.

            Quaternion step = Quaternion.CreateFromYawPitchRoll(-Input.Look.X * YawStep * dt, -Input.Look.Y * PitchStep * dt, -Input.Movement.X * RollStep * dt);
            this.Orientation = this.Orientation * step;
            
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
