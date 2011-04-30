using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Canyon.Screens;
using Canyon.Misc;
using Canyon.Screens.Canyon;
using Canyon.CameraSystem;
using System;

namespace Canyon
{
    public class CanyonGame : Microsoft.Xna.Framework.Game
    {
        public static CanyonGame Instance { get; private set; }
        public static SimpleConsole Console { get; private set; }
        public static ScreenManager Screens { get; private set; }
        public static ICamera Camera { get; private set; }

        private GraphicsDeviceManager graphics;

        public CanyonGame()
        {
            CanyonGame.Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Window and framerate settings:
#if !DEBUG
            this.graphics.IsFullScreen = true;
            this.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            this.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#else // !DEBUG
            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 5);
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

            // Add basics:
            this.Components.Add(new FrameCounter(this));

            // Initialize the sceen manager:
            Screen start = new GameScreen(this);
            this.Components.Add(CanyonGame.Screens = new ScreenManager(this, start));

            // Create a simple camera:
            CanyonGame.Camera = new BaseCamera(Vector3.UnitZ, Vector3.Zero, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000.0f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Crimson);

            base.Draw(gameTime);
        }
    }
}
