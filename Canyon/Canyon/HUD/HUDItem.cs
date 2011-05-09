using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Canyon.Entities;

namespace Canyon.HUD
{
    public enum HUDType
    {
        FirstPerson = 1,
        ThirdPerson = 2,
        Both = 3,
    }
    public abstract class HUDItem : DrawableGameComponent
    {
        public abstract HUDType Type { get; }
        protected Display display;
        protected Player player;

        public Vector2 Position { get; protected set; }

        public HUDItem(Game game, Display d, Player p)
            : base(game)
        {
            this.display = d;
            this.player = p;
        }

        public override void Initialize()
        {
            SetPosition();
            base.Initialize();
            GraphicsDevice.DeviceReset += delegate(object s, EventArgs a)
            {
                this.SetPosition();
            };
        }

        protected virtual void SetPosition()
        {
            this.Position = Vector2.Zero;
        }

    }
}
