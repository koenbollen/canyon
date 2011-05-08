using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Canyon.CameraSystem;
using Canyon.Misc;
using System;
using System.Linq;


namespace Canyon.Entities
{
    public class Player : DrawableGameComponent, IEntity
    {
        public const float PitchStep = MathHelper.Pi/8;
        public const float YawStep = MathHelper.Pi/16;
        public const float RollStep = MathHelper.Pi;
        public const float Speed = 2500f;
        public const float Drag = 0.97f;

        private YawPitchRoll orientation;

        public Vector3 Position { get; protected set; }
        public Quaternion Orientation { get { return orientation.Orientation; } }

        public Vector3 Up { get { return Vector3.Transform(Vector3.Up, this.Orientation); } }
        public Vector3 Right { get { return Vector3.Transform(Vector3.Right, this.Orientation); } }
        public Vector3 Left { get { return Vector3.Transform(Vector3.Left, this.Orientation); } }
        public Vector3 Forward { get { return Vector3.Transform(Vector3.Forward, this.Orientation); } }

        // Semi physics:
        public Vector3 Velocity { get; protected set; }
        public Vector3 Acceleration { get; protected set; }
        public Vector3 Force { get; protected set; }
        public float Mass { get; protected set; }
        public float Thrust { get; protected set; }

        private Model model;

        private SpringFollowCamera followCamera;

        protected InputManager Input;
        public Player(Game game, Vector3 position)
            : base(game)
        {
            this.Position = position;

            followCamera = new SpringFollowCamera(Game);
            CanyonGame.Screens.ActiveScreen.Components.Add(followCamera);
        }

        public override void Initialize()
        {
            Input = CanyonGame.Input;
            Input.CenterMouse = true;

            this.Mass = 15.0f;

            CanyonGame.Instance.ChangeCamera(followCamera);
            this.UpdateCamera();
            this.followCamera.Reset();

            #region Debug commands: yaw, pitch, roll
#if DEBUG
            CanyonGame.Console.Commands["yaw"] = delegate(Microsoft.Xna.Framework.Game game, string[] argv, GameTime gameTime)
            {
                if (argv.Length < 2)
                {
                    CanyonGame.Console.WriteLine("usage: yaw <angle in degrees>");
                    return;
                }
                this.orientation.Yaw = MathHelper.WrapAngle(MathHelper.ToRadians(float.Parse(argv[1], System.Globalization.CultureInfo.InvariantCulture)));
            };
            CanyonGame.Console.Commands["pitch"] = delegate(Microsoft.Xna.Framework.Game game, string[] argv, GameTime gameTime)
            {
                if (argv.Length < 2)
                {
                    CanyonGame.Console.WriteLine("usage: pitch <angle in degrees>");
                    return;
                }
                this.orientation.Pitch = MathHelper.WrapAngle(MathHelper.ToRadians(float.Parse(argv[1], System.Globalization.CultureInfo.InvariantCulture)));
            };
            CanyonGame.Console.Commands["roll"] = delegate(Microsoft.Xna.Framework.Game game, string[] argv, GameTime gameTime)
            {
                if (argv.Length < 2)
                {
                    CanyonGame.Console.WriteLine("usage: roll <angle in degrees>");
                    return;
                }
                this.orientation.Roll = MathHelper.WrapAngle(MathHelper.ToRadians(float.Parse(argv[1], System.Globalization.CultureInfo.InvariantCulture)));
            };
#endif
            #endregion

#if DEBUG
            Grid g = Game.Components.OfType<Grid>().First();
            if (g != null) g.Follow = this;
#endif

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

            this.UpdateOrientation(dt);
            UpdatePhysics(dt);
            UpdateCamera();

            base.Update(gameTime);
        }

        private void UpdatePhysics(float dt)
        {
            Thrust = Speed * MathHelper.Clamp(-Input.Movement.Z, 0, 1);
            this.Force += this.Forward * Thrust;
            this.Acceleration += Force / Mass;

            this.Velocity += this.Acceleration * dt;
            Velocity *= Drag;

            this.Position += this.Velocity * dt;

            ClearForces();
        }

        private void ClearForces()
        {
            this.Force = Vector3.Zero;
            this.Acceleration = Vector3.Zero;
        }

        private void UpdateOrientation(float dt)
        {
            Quaternion yaw, pitch, roll;
            yaw = pitch = roll = Quaternion.Identity;

            if (Input.Look.X != 0)
                orientation.Yaw += -Input.Look.X * YawStep * dt;
            if (Input.Look.Y != 0)
                orientation.Pitch += -Input.Look.Y * PitchStep * dt;
            if (Input.Movement.X != 0)
                orientation.Roll += -Input.Movement.X * RollStep * dt;
        }

        private void UpdateCamera()
        {
            followCamera.Target = this.Position;
            followCamera.Direction = this.Forward;
            followCamera.Up = this.Up;
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix world = Matrix.CreateFromQuaternion(Orientation) * Matrix.CreateTranslation(this.Position);

            Matrix[] transforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * world;
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
