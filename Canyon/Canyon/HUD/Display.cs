using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Canyon.Entities;

namespace Canyon.HUD
{
    public enum HUDState
    {
        FirstPerson = 1,
        ThirdPerson = 2,
    }
    public class Display : DrawableGameComponent
    {
        public HUDState State { get; set; }

        internal SpriteBatch Batch { get; private set; }
        internal SpriteFont Font { get; private set; }
        private List<HUDItem> HUDItems;
        private Player player;

        public Display(Game game, Player p)
            :base(game)
        {
            this.player = p;
            this.State = HUDState.ThirdPerson;
            HUDItems = new List<HUDItem>();
        }

        protected override void LoadContent()
        {
            Batch = new SpriteBatch(GraphicsDevice);
            Font = Game.Content.Load<SpriteFont>("Fonts/default");
            AddItems();
            for (int i = 0; i < HUDItems.Count; i++)
                HUDItems[i].Initialize();
            base.LoadContent();
        }

        private void AddItems()
        {
            this.HUDItems.Add( new Speed(Game, this, player) );
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < HUDItems.Count; i++)
                    HUDItems[i].Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < HUDItems.Count; i++)
                if (((int)this.State & (int)HUDItems[i].Type) != 0)
                    HUDItems[i].Draw(gameTime);
            base.Draw(gameTime);
        }
    }
   
}
