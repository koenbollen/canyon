using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Canyon.Misc;

namespace Canyon
{
    public class InputManager : GameComponent
    {
        public Vector3 Movement { get; protected set; }
        public Vector2 Look { get; protected set; }
        public float Roll { get; protected set; }

        public bool CenterMouse { get; set; }
        public float MouseSensitivity { get; set; }

        private KeyboardState pkbs;
        private KeyboardState ckbs;
        private MouseState pms;
        private MouseState cms;
        private GamePadState pgps;
        private GamePadState cgps;

        private Vector2 center;

        public float ScrollWheelValue
        {
            get
            {
                return this.cms.ScrollWheelValue;
            }
        }
        public float ScrollWheelDelta
        {
            get
            {
                return this.cms.ScrollWheelValue - this.pms.ScrollWheelValue;
            }
        }

        public bool TogglePlayerMode
        {
            get
            {
                return this.IsJustDown(Keys.LeftShift) || this.IsJustDown(Buttons.LeftShoulder);
            }
        }

        public bool LaunchRocket
        {
            get
            {
                return cms.RightButton == ButtonState.Pressed && pms.RightButton == ButtonState.Released;
            }
        }

        public InputManager(Game game)
            : base(game)
        {
            CenterMouse = false;
            MouseSensitivity = 100.0f;
        }

        public override void Initialize()
        {
            ckbs = Keyboard.GetState();
            cms = Mouse.GetState();
            cgps = GamePad.GetState(PlayerIndex.One);

            // Calculate center of the screen and keep it updated:
            GraphicsDevice_DeviceReset(null, null);
            Game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);

            Mouse.SetPosition((int)center.X, (int)center.Y);

            base.Initialize();
            CanyonGame.Console.Trace("InputManager initialized.");
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            if( center != Vector2.Zero )
                CanyonGame.Console.Trace("Device resetted, updated center vector in InputManager.");
            center = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this.pkbs = this.ckbs;
            this.pms = this.cms;
            this.pgps = this.cgps;

            // Keep keyboardstate on hold if the console is visible:
            this.ckbs = CanyonGame.Console.Open ? this.pkbs : Keyboard.GetState();
            // The rest still works:
            this.cms = Mouse.GetState();
            this.cgps = GamePad.GetState(PlayerIndex.One);

            this.HandleLook(dt);
            this.HandleMovement();
            this.HandleRoll();

            base.Update(gameTime);
        }

        private void HandleLook(float dt)
        {
            Vector2 delta = new Vector2(cms.X - center.X, cms.Y - center.Y);
            if (!delta.IsValid()) // check for NaN & Inf.
                delta = Vector2.Zero;
            Vector2 RightThumb = new Vector2( cgps.ThumbSticks.Right.X, -cgps.ThumbSticks.Right.Y);

            this.Look = (delta / MouseSensitivity) + RightThumb;
            this.Look = Vector2.Clamp(this.Look, -Vector2.One, Vector2.One);

            if (CenterMouse)
            {
                float length = delta.Length();
                if (length > MouseSensitivity)
                    length = MouseSensitivity;
                else if (length < MouseSensitivity * 0.6f)
                    length -= (MouseSensitivity/2) * dt;
                else if (length < 0.01f)
                    length = 0;
                delta = delta.SafeNormalize() * length;
                Mouse.SetPosition((int)Math.Round(center.X + delta.X), (int)Math.Round(center.Y + delta.Y));
            }
        }

        private void HandleMovement()
        {
            this.Movement = Vector3.Zero;
            if (ckbs.IsKeyDown(Keys.W))
                this.Movement += Vector3.Forward;
            if (ckbs.IsKeyDown(Keys.S))
                this.Movement += Vector3.Backward;
            if (ckbs.IsKeyDown(Keys.A))
                this.Movement += Vector3.Left;
            if (ckbs.IsKeyDown(Keys.D))
                this.Movement += Vector3.Right;
            Vector2 LeftThumb = cgps.ThumbSticks.Left;
            this.Movement += Vector3.Forward * LeftThumb.Y;
            this.Movement += Vector3.Right * LeftThumb.X;

            // Lift:
            if (ckbs.IsKeyDown(Keys.Space))
                this.Movement += Vector3.Up;
            this.Movement += Vector3.Up * cgps.Triggers.Left;

            this.Movement = Vector3.Clamp(this.Movement, -Vector3.One, Vector3.One);
        }

        private void HandleRoll()
        {
            this.Roll = 0;
            if (ckbs.IsKeyDown(Keys.Q) || cgps.IsButtonDown(Buttons.LeftShoulder))
                this.Roll += -1;
            if (ckbs.IsKeyDown(Keys.E) || cgps.IsButtonDown(Buttons.RightShoulder))
                this.Roll +=  1;
        }

        public bool IsKeyDown(Keys key)
        {
            return ckbs.IsKeyDown(key);
        }
        public bool IsKeyUp(Keys key)
        {
            return ckbs.IsKeyUp(key);
        }

        public bool IsJustDown(Keys key)
        {
            return pkbs.IsKeyUp(key) && ckbs.IsKeyDown(key);
        }
        public bool IsJustDown(Buttons button)
        {
            return pgps.IsButtonUp(button) && cgps.IsButtonDown(button);
        }
        public bool IsJustUp(Keys key)
        {
            return pkbs.IsKeyDown(key) && ckbs.IsKeyUp(key);
        }

    }
}

