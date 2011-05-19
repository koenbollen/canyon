using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Canyon.Entities;

namespace Canyon.HUD
{
    public abstract class Item : DrawableGameComponent
    {
        protected Display display;

        public Vector2 Position { get; protected set; }

        public Item(Game game, Display d)
            : base(game)
        {
            this.display = d;
        }

        public override void Initialize()
        {
            base.Initialize();
            SetPosition();
            GraphicsDevice.DeviceReset += delegate(object s, EventArgs a)
            {
                this.SetPosition();
            };
        }

        protected virtual void SetPosition()
        {
            this.Position = Vector2.Zero;
        }

        public abstract bool ShowInMode(PlayerMode mode);

    }
}
