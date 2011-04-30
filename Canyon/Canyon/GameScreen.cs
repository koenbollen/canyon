using Microsoft.Xna.Framework;
using Canyon.Screens;
using Canyon.Entities;

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
            //this.Components.Add(this.player = new Player(Game, Vector3.Zero));
            base.Initialize();
        }
    }
}
