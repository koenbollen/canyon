using System;
using System.Globalization;
using System.Threading;
using Canyon.CameraSystem;
using Canyon.Misc;
using Canyon.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Canyon
{
    public class CanyonGame : Microsoft.Xna.Framework.Game
    {
        public static CanyonGame Instance { get; private set; }
        public static SimpleConsole Console { get; private set; }
        public static ScreenManager Screens { get; private set; }
        public static ICamera Camera { get; set; }

        private GraphicsDeviceManager graphics;

        public CanyonGame()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            CanyonGame.Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Window and framerate settings:
            graphics.PreferMultiSampling = true;
#if !DEBUG
            this.graphics.IsFullScreen = true;
            this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#else // !DEBUG
            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1);
            //this.IsFixedTimeStep = false;
            this.graphics.SynchronizeWithVerticalRetrace = false;
#endif // !DEBUG


#if !XBOX
            Window.Title = "Canyon Shooter, by Koen Bollen (2011, HvA)";
#endif // !XBOX
        }

        protected override void Initialize()
        {
            // Start with the console, always the most important thing in a game:
            this.Components.Add(CanyonGame.Console = new SimpleConsole(this));

#if DEBUG
            this.Components.Add(new FrameCounter(this));
            this.Components.Add(new VectorDrawer(this));
#endif // !DEBUG

            // Initialize the sceen manager:
            Screen start = new GameScreen(this);
            this.Components.Add(CanyonGame.Screens = new ScreenManager(this, start));

            // Create a simple camera:
            DebugCamera camera = new DebugCamera(this, new Vector3(-20, 60, -20), -MathHelper.Pi/4*3, -MathHelper.Pi/8, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000.0f);
            this.Components.Add(camera);
            CanyonGame.Camera = camera;

            base.Initialize();
            GC.Collect(); // TODO: Move to LoadScreen
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (gameTime.TotalGameTime.TotalMinutes > 2)
                this.Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            base.Draw(gameTime);
        }
    }
}
