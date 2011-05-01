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
    public delegate void OnCameraChanged( ICamera prev, ICamera current );
    public class CanyonGame : Microsoft.Xna.Framework.Game
    {
        public static CanyonGame Instance { get; private set; }
        public static SimpleConsole Console { get; private set; }
        public static InputManager Input { get; private set; }
        public static ScreenManager Screens { get; private set; }
        public static ICamera Camera { get; private set; }

        public event OnCameraChanged CameraChanged;

        public static float AspectRatio = 4.0f / 3.0f;
        public static float NearPlane = 0.1f;
        public static float FarPlane = 1000.0f;

        public bool DoExit;

        private GraphicsDeviceManager graphics;

        public CanyonGame()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // why the hell isn't this default in .net applications?!1

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
            CanyonGame.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            // Start with the console, always the most important thing in a game:
            this.Components.Add(CanyonGame.Console = new SimpleConsole(this));
            this.Components.Add(CanyonGame.Input = new InputManager(this));

#if DEBUG
            this.Components.Add(new FrameCounter(this));
            this.Components.Add(new VectorDrawer(this));
#endif // !DEBUG

            // Initialize the sceen manager:
            Screen start = new GameScreen(this);
            this.Components.Add(CanyonGame.Screens = new ScreenManager(this, start));

            // Create a simple camera:
#if DEBUG
            DebugCamera camera = new DebugCamera(this, new Vector3(-20, 60, -20), -MathHelper.Pi/4*3, -MathHelper.Pi/8);
            this.Components.Add(camera);
            CanyonGame.Camera = camera;

            ICamera last = camera;
            CanyonGame.Console.Commands["camera_debug"] = delegate( Game game, string[] argv, GameTime gameTime )
            {
                if (CanyonGame.Camera != camera)
                {
                    last = CanyonGame.Camera;
                    CanyonGame.Instance.ChangeCamera(camera);
                }else
                    CanyonGame.Camera = last;
            };
#else // DEBUG
            CanyonGame.Camera = new FallbackCamera();
#endif // DEBUG


            base.Initialize();
            GC.Collect(); // TODO: Move to LoadScreen
        }

        protected override void LoadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (DoExit || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
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

        public void ChangeCamera(ICamera camera )
        {
            ICamera old = CanyonGame.Camera;
            CanyonGame.Camera = camera;
            if (CameraChanged != null)
                CameraChanged(old, CanyonGame.Camera);
        }
    }
}
