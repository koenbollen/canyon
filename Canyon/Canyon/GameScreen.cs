using Microsoft.Xna.Framework;
using Canyon.Screens;
using Canyon.Entities;
using Canyon.Environment;

namespace Canyon
{
    public class GameScreen : Screen
    {
        public const string DefaultMap = "riemers";
        public const string MapDirectory = "Heightmaps";

        private string mapname;
        private Player player;

        public GameScreen(Game game, string mapname=DefaultMap)
            :base(game)
        {
            this.mapname = mapname;
        }

        public override void Initialize()
        {
            CanyonGame.Console.Trace("Loading map: " + this.mapname + "...");
            this.Components.Add(new Terrain(Game, MapDirectory + "/" + this.mapname));
            this.Components.Add(this.player = new Player(Game, Vector3.One* 30));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

    }
}
