using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Canyon.Misc
{
    /// <summary>
    /// Display the current framerate in the topleft corner of the screen.
    /// </summary>
    public class FrameCounter : DrawableGameComponent
    {
        SpriteBatch batch;
        SpriteFont font;

        public const string format = "fps {0:N}";

        private Vector2 position = new Vector2(10, 10);
        private Color color = new Color( 0xed, 0xaa, 0x09 );

        private double fps;


        public FrameCounter(Game game)
            : base(game)
        {
            DrawOrder = 100;
        }

        protected override void LoadContent()
        {
            batch = new SpriteBatch(GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("Fonts/default");

            this.position = new Vector2(10, GraphicsDevice.Viewport.Height - 10 - font.LineSpacing);

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            // Update FPS:
            fps = gameTime.ElapsedGameTime.TotalSeconds * 0.05 + fps * 0.95;

            // Build text
            string text = string.Format(FrameCounter.format, 1.0/this.fps);

            // Draw w/ shadow:
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            batch.DrawString(this.font, text, this.position + Vector2.One, new Color(0, 0, 0, 0.8f));
            batch.DrawString(this.font, text, this.position, this.color);
            batch.End();

            base.Draw(gameTime);
        }
    }
}
