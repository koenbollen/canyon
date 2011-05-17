using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.HUD
{
    public class Crosshair : Item
    {

        private Texture2D image;

        public Crosshair(Game game, Display d)
            :base( game, d )
        {
        }

        protected override void SetPosition()
        {
            this.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            if (image != null)
            {
                this.Position -= new Vector2(image.Width, image.Height) / 2;
            }
        }

        protected override void LoadContent()
        {
            image = Game.Content.Load<Texture2D>("Sprites/crosshair");
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            display.Batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            display.Batch.Draw(this.image, this.Position, Color.White);
            display.Batch.End();
            base.Draw(gameTime);
        }

        public override bool ShowInMode(Entities.PlayerMode mode)
        {
            return mode == Entities.PlayerMode.Firstperson;
        }
    }

}
