using System.Collections.Generic;
using Canyon.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.HUD
{
    public class Display : DrawableGameComponent
    {
        internal SpriteBatch Batch { get; private set; }
        internal SpriteFont Font { get; private set; }
        internal Player Player { get; private set; }

        private List<Item> Items;

        public Display(Game game, Player p)
            :base(game)
        {
            this.Player = p;
            Items = new List<Item>();
        }

        protected override void LoadContent()
        {
            Batch = new SpriteBatch(GraphicsDevice);
            Font = Game.Content.Load<SpriteFont>("Fonts/default");
            AddItems();
            for (int i = 0; i < Items.Count; i++)
                Items[i].Initialize();
            base.LoadContent();
        }

        private void AddItems()
        {
            this.Items.Add(new Speed(Game, this));
            this.Items.Add(new Crosshair(Game, this));
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < Items.Count; i++)
                    Items[i].Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].ShowInMode(Player.CurrentMode) )
                    Items[i].Draw(gameTime);
            base.Draw(gameTime);
        }
    }
   
}
