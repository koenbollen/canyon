﻿using Microsoft.Xna.Framework;
using Canyon.Screens;
using Canyon.Entities;
using Canyon.Environment;

namespace Canyon
{
    public class GameScreen : Screen
    {
        private Player player;

        public GameScreen(Game game)
            :base(game)
        {
        }

        public override void Initialize()
        {
            this.Components.Add(new Terrain(Game, "Heightmaps/riemers"));
            this.Components.Add(this.player = new Player(Game, Vector3.One* 30));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

    }
}
