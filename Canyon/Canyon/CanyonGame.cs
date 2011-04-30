using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Canyon.Screens;
using Canyon.Misc;
using Canyon.Screens.Canyon;

namespace Canyon
{
    public class CanyonGame : Microsoft.Xna.Framework.Game
    {
        public static CanyonGame Instance { get; private set; }
        public static SimpleConsole Console { get; private set; }
        public static ScreenManager Screens { get; private set; }

        private GraphicsDeviceManager graphics;

        public CanyonGame()
        {
            CanyonGame.Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.Components.Add(CanyonGame.Console = new SimpleConsole(this));

            Screen start = new GameScreen(this);
            this.Components.Add(CanyonGame.Screens = new ScreenManager(this, start));

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
