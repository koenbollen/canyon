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

        private KeyboardState pkbs;
        private KeyboardState ckbs;
        private MouseState pms;
        private MouseState cms;
        private GamePadState pgps;
        private GamePadState cgps;

        private Vector2 screenFactor;
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

        public InputManager(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            ckbs = Keyboard.GetState();
            cms = Mouse.GetState();
            cgps = GamePad.GetState(PlayerIndex.One);

            CenterMouse = false;

            // Calculate center of the screen and keep it updated:
            GraphicsDevice_DeviceReset(null, null);
            Game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);

            base.Initialize();
            CanyonGame.Console.Trace("InputManager initialized.");
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            if( center != Vector2.Zero )
                CanyonGame.Console.Trace("Device resetted, updated center vector and screen factor in InputManager.");
            center = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            screenFactor = new Vector2( 
                (500.0f * Game.GraphicsDevice.Viewport.AspectRatio) / Game.GraphicsDevice.Viewport.Width,
                500.0f / Game.GraphicsDevice.Viewport.Height );
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
            // frag mouse movement on different frame rates and resolutions...
            Vector2 delta = new Vector2(cms.X - center.X, cms.Y - center.Y) * screenFactor / dt / 1000f;
            if (!delta.IsValid())
                delta = Vector2.Zero;
            Vector2 RightThumb = new Vector2( cgps.ThumbSticks.Right.X, -cgps.ThumbSticks.Right.Y);
            this.Look = delta + RightThumb;

            if (CenterMouse)
                Mouse.SetPosition((int)this.center.X, (int)this.center.Y);
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
        public bool IsJustUp(Keys key)
        {
            return pkbs.IsKeyDown(key) && ckbs.IsKeyUp(key);
        }
    }
}

