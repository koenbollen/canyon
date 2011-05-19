using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Canyon.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.HUD
{
    public class Speed : Item
    {
        public Speed( Game game, Display d )
            :base( game, d )
        {
        }

        protected override void SetPosition()
        {
            this.Position = Vector2.One * 10;
        }

        public override void Draw(GameTime gameTime)
        {
            display.Batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            display.Batch.DrawString(display.Font, string.Format("Speed: {0:N}", Vector3.Dot(display.Player.Forward, display.Player.Velocity)), this.Position + Vector2.One, Color.Black);
            display.Batch.DrawString(display.Font, string.Format("Speed: {0:N}", Vector3.Dot(display.Player.Forward, display.Player.Velocity)), this.Position, Color.Yellow);
            display.Batch.End();
            base.Draw(gameTime);
        }

        public override bool ShowInMode(PlayerMode mode)
        {
            return true;
        }
    }
}
