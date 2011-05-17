using System;
using System.Linq;
using Canyon.CameraSystem;
using Canyon.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Canyon.HUD;
using System.Collections.Generic;


namespace Canyon.Entities
{
    public enum PlayerMode
    {
        Firstperson,
        Thirdperson
    }

    public class Player : DrawableGameComponent, IEntity, IFollowable
    {
        public const float PitchStep = MathHelper.Pi / 4; // to the power of 2
        public const float YawStep = MathHelper.Pi / 4; // to the power of 2
        public const float RollStep = MathHelper.Pi/2;
        public const float RollCorrection = MathHelper.Pi / 16;
        public const float RollAffect = YawStep * 10;
        public const float Speed = 250f;
        public const float Drag = 1.9f;

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

        public float AntiGravity { get; set; }

        private Model model;

        public PlayerMode CurrentMode { get; private set; }

        private Dictionary<PlayerMode, IFollowCamera> Cameras;

        protected InputManager Input;
        protected GameScreen screen;
        public Player(GameScreen screen, Vector3 position)
            : base(screen.Game)
        {
            this.screen = screen;
            this.Position = position;

            // Setup PlayerMode and cameras:
            CurrentMode = PlayerMode.Thirdperson;
            Cameras = new Dictionary<PlayerMode, IFollowCamera>();
            Cameras[PlayerMode.Firstperson] = new FirstpersonCamera(Game, this);
            Cameras[PlayerMode.Thirdperson] = new SpringChaseCamera(Game, this);
            foreach (IFollowCamera c in Cameras.Values)
                if (c is IGameComponent)
                    screen.Components.Add(c as IGameComponent);
        }

        public override void Initialize()
        {
            Input = CanyonGame.Input;
            Input.CenterMouse = true;

            this.Mass = 15.0f;
            this.AntiGravity = 1;

            IFollowCamera fc = Cameras[CurrentMode];
            CanyonGame.Instance.ChangeCamera(fc);
            fc.HardSet();

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

            if (Input.TogglePlayerMode)
                ToggleMode();

            this.UpdateOrientation(dt);
            RollLogics(dt);
            UpdateGravity(dt);
            UpdatePhysics(dt);
            ClearForces();

            if (Cameras[CurrentMode] is IUpdateable)
                (Cameras[CurrentMode] as IUpdateable).Update(gameTime);


            if (Input.IsJustDown(Keys.Space))
            {
                float frac = 0f;
                Vector3 normal = Vector3.Zero;
                Ray r = new Ray(Position, Forward);
                bool hit = screen.Terrain.Intersect(r, ref frac, ref normal);
                if (hit)
                {
                    (normal).Draw(r.Position + (r.Direction * frac), Color.Black);
                    (r.Direction * frac).Draw(r.Position, Color.Green);
                }
                else
                    (r.Direction * 100).Draw(r.Position, Color.Red);
            }


            base.Update(gameTime);
        }

        private void ToggleMode()
        {
            IFollowCamera p = Cameras[CurrentMode];
            if (CurrentMode == PlayerMode.Firstperson)
                CurrentMode = PlayerMode.Thirdperson;
            else
                CurrentMode = PlayerMode.Firstperson;
            IFollowCamera c = Cameras[CurrentMode];
            c.HardSet(p.GetCurrentStateAsTarget());
            CanyonGame.Instance.ChangeCamera(c);
        }

        private void UpdateGravity(float dt)
        {
            if (Input.IsJustDown(Keys.R))
            {
                this.AntiGravity += .1f;
            }
            if (Input.IsJustDown(Keys.F))
            {
                this.AntiGravity -= .1f;
            }

            this.Force += CanyonGame.Gravity;
            this.AntiGravity = MathHelper.Clamp(this.AntiGravity, 0, 2);
            this.Force += -CanyonGame.Gravity * this.AntiGravity;
        }


        /// <summary>
        /// Apply some logics on the Roll of the ship. When the 
        /// roll isn't level the angle will result in a certain 
        /// amount of yaw and the roll is leveled back to 0.
        /// </summary>
        /// <param name="dt"></param>
        private void RollLogics(float dt)
        {
            float roll = this.orientation.Roll;
            float abspitch = Math.Abs(this.orientation.Pitch);
            //Vector3 up = Vector3.Up;
            //if (this.Up.Y < 0)
            //    up = Vector3.Down;

            //float amount = Math.Abs(roll) / MathHelper.Pi * 2;
            //if (Math.Abs(roll) > MathHelper.PiOver2)
            //    amount = 2.0f - amount;

            //float direction = roll < 0 ? -1 : 1;
            //if (abspitch > MathHelper.PiOver2)
            //    direction *= -1;

            //float speedFactor = Math.Min(1, Vector3.Dot(this.Forward, Velocity));

            //this.orientation.Yaw += amount * direction * speedFactor * RollAffect * dt;



            // Slowly roll to level:
            float level = Math.Abs(Math.Abs(abspitch - MathHelper.PiOver2) - MathHelper.PiOver2);
            if (level < MathHelper.PiOver4)
            {
                if (roll > 0 && roll < MathHelper.PiOver4)
                {
                    this.orientation.Roll = Math.Max(0, roll - RollCorrection * dt);
                }
                else if (roll > -MathHelper.PiOver4 && roll < 0)
                {
                    this.orientation.Roll = Math.Min(0, roll + RollCorrection * dt);
                }
            }
        }

        private void UpdatePhysics(float dt)
        {
            Thrust = Speed * MathHelper.Clamp(-Input.Movement.Z, 0, 1);
            this.Force += this.Forward * Thrust;
            this.Acceleration += Force / Mass;
            this.Acceleration -= Velocity * Drag;

            this.Velocity += this.Acceleration * dt;
            this.Position += this.Velocity * dt;

        }

        private void ClearForces()
        {
            this.Force = Vector3.Zero;
            this.Acceleration = Vector3.Zero;
        }

        private void UpdateOrientation(float dt)
        {
            float x2 = (Input.Look.X * Input.Look.X);
            if (Input.Look.X < 0)
                x2 *= -1;
            float y2 = (Input.Look.Y * Input.Look.Y);
            if (Input.Look.Y < 0)
                y2 *= -1;
            if (Input.Look.X != 0)
            {
                if (this.Up.Y < 0)
                    orientation.Yaw += x2 * YawStep * dt;
                else
                    orientation.Yaw += -x2 * YawStep * dt;
            }
            if (Input.Look.Y != 0)
            {
                if ( Math.Abs(this.orientation.Roll) > MathHelper.PiOver2  )
                    orientation.Pitch += y2 * PitchStep * dt;
                else
                    orientation.Pitch += -y2 * PitchStep * dt;
            }
            if (Input.Movement.X != 0)
                orientation.Roll += -Input.Movement.X * RollStep * dt;
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
