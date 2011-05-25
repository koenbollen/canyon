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

    public class Player : BaseEntity
    {
        public const float PitchStep = MathHelper.Pi / 4; // to the power of 2
        public const float YawStep = MathHelper.Pi / 4; // to the power of 2
        public const float RollStep = MathHelper.Pi/2;
        public const float RollCorrection = MathHelper.Pi / 16;
        public const float Speed = 250f;
        //TODO: public const float Drag = 1.9f;

        public float Thrust { get; protected set; }

        public float AntiGravity { get; set; }

        public PlayerMode CurrentMode { get; private set; }

        private Rocket.RocketDeploy rockets;

        private Dictionary<PlayerMode, IFollowCamera> Cameras;

        protected InputManager Input;
        public Player(GameScreen screen, Vector3? position)
            : base(screen, position)
        {
            // Setup PlayerMode and cameras:
            CurrentMode = PlayerMode.Thirdperson;
            Cameras = new Dictionary<PlayerMode, IFollowCamera>();
            Cameras[PlayerMode.Firstperson] = new FirstpersonCamera(Game, this);
            Cameras[PlayerMode.Thirdperson] = new SpringChaseCamera(Game, this);
            foreach (IFollowCamera c in Cameras.Values)
                if (c is IGameComponent)
                    screen.Components.Add(c as IGameComponent);
            this.Drag = 1.5f;
            this.Mass = 15.0f;
            this.AntiGravity = 1;
            this.AffectedByGravity = false;

            rockets = new Rocket.RocketDeploy(screen, this);
            screen.Components.Add(rockets);
        }

        public override void Initialize()
        {
            Input = CanyonGame.Input;
            Input.CenterMouse = true;

            IFollowCamera fc = Cameras[CurrentMode];
            CanyonGame.Instance.ChangeCamera(fc);
            fc.HardSet();

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
            float thrust = Speed * MathHelper.Clamp(-Input.Movement.Z, 0, 1);
            this.AddForce(this.Forward * thrust);

            if (Cameras[CurrentMode] is IUpdateable)
                (Cameras[CurrentMode] as IUpdateable).Update(gameTime);

            if (Input.IsJustDown(Keys.Space))
            {
                float frac = 0f;
                Vector3 normal = Vector3.Zero;
                Ray r = new Ray(Position, Forward);
                bool hit = Screen.Terrain.Intersect(r, ref frac, ref normal);
                if (hit)
                {
                    (normal).Draw(r.Position + (r.Direction * frac), Color.Black);
                    (r.Direction * frac).Draw(r.Position, Color.Green);
                }
                else
                    (r.Direction * 100).Draw(r.Position, Color.Red);
            }

            if (Input.LaunchRocket)
            {
                this.rockets.Deploy();
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

            this.AddForce(CanyonGame.Gravity);
            this.AntiGravity = MathHelper.Clamp(this.AntiGravity, 0, 2);
            this.AddForce(-CanyonGame.Gravity * this.AntiGravity);
        }


        /// <summary>
        /// Apply some logics on the Roll of the ship. When the 
        /// roll isn't level the roll is leveled back to 0.
        /// </summary>
        /// <param name="dt"></param>
        private void RollLogics(float dt)
        {
            float roll = 0;
            float realroll = this.Orientation.GetRoll();
            float abspitch = Math.Abs(this.Orientation.GetPitch());
            
            // Slowly roll to level:
            float level = Math.Abs(Math.Abs(abspitch - MathHelper.PiOver2) - MathHelper.PiOver2);
            if (level < MathHelper.PiOver4)
            {
                if (realroll > 0 && realroll < MathHelper.PiOver4)
                    roll = Math.Max(0, realroll - RollCorrection * dt);
                else if (realroll > -MathHelper.PiOver4 && realroll < 0)
                    roll = Math.Min(0, realroll + RollCorrection * dt);
                else
                    roll = realroll;
                roll = roll - realroll;
            }
            this.Orientation *= Quaternion.CreateFromYawPitchRoll(0, 0, roll);
        }

        private void UpdateOrientation(float dt)
        {
            float yaw, pitch, roll;
            yaw = pitch = roll = 0;

            float x2 = (Input.Look.X * Input.Look.X);
            if (Input.Look.X < 0)
                x2 *= -1;
            float y2 = (Input.Look.Y * Input.Look.Y);
            if (Input.Look.Y < 0)
                y2 *= -1;

            if (Input.Look.X != 0)
                yaw += -x2 * YawStep * dt;
            if (Input.Look.Y != 0)
                pitch += -y2 * PitchStep * dt;
            if (Input.Movement.X != 0)
                roll += -Input.Movement.X * RollStep * dt;

            this.Orientation *= Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        protected override void ApplyEffect(BasicEffect effect)
        {
            effect.DiffuseColor = Color.DarkMagenta.ToVector3();
            base.ApplyEffect(effect);
        }

    }
}
