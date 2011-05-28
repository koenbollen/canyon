﻿using Microsoft.Xna.Framework;
using Canyon.Screens;
using Canyon.Entities;
using Canyon.Environment;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Text;
using Canyon.HUD;
using Canyon.Misc;

namespace Canyon
{
    public class GameScreen : Screen
    {
        public const string DefaultMap = "riemers";
        public const string MapDirectory = "Heightmaps";

        private string mapname;
        private Player player;
        public Terrain Terrain { get; private set; }

        private string changemap;

        public GameScreen(Game game, string mapname=DefaultMap)
            :base(game)
        {
            this.mapname = mapname;
            this.changemap = null;
        }

        public override void Initialize()
        {
            RegisterCommands();

            CanyonGame.Console.Trace("Loading map: " + this.mapname + "...");
            this.Components.Add(Terrain=new Terrain(Game, MapDirectory + "/" + this.mapname));

            this.Components.Add(this.player = new Player(this, Vector3.One * 30));
            this.Components.Add(new Display(Game, player));

#if DEBUG
            this.Components.Add(new Grid(Game, (int)(CanyonGame.FarPlane / 20), 10, this.player));
#endif

            this.Components.Add(new MarkerPath(this, Vector3.One * 30 + Vector3.Forward * 10, Vector3.Backward, 20));

            Vector3 box = new Vector3(2, 5, 4);
            Matrix world = Matrix.CreateRotationX(MathHelper.Pi / 16) * Matrix.CreateTranslation(new Vector3(10, 32, 8));

            Vector3[] vertices = new Vector3[] {
                                     new Vector3( 10, 30, 0), 
                                     new Vector3( 8, 35, 6 ),
                                     new Vector3( 10, 40, 4 ), 
                                 };

            Color tricolor = Color.White;
            if (MathUtils.OverlapBoxTriangle(world, box, vertices))
                tricolor = Color.Black;

            VectorDrawer.AddBox(box, world);
            VectorDrawer.AddVector(vertices[0], vertices[1] - vertices[0], tricolor);
            VectorDrawer.AddVector(vertices[1], vertices[2] - vertices[1], tricolor);
            VectorDrawer.AddVector(vertices[2], vertices[0] - vertices[2], tricolor);

            base.Initialize();
        }

        private void RegisterCommands()
        {

            // Commands added after this point are only added once.
            if (CanyonGame.Console.Commands.ContainsKey("maps"))
                return;

            #region Command 'map' 
            CanyonGame.Console.Commands["map"] = delegate(Game game, string[] argv, GameTime gameTime)
            {
                if (argv.Length < 2)
                {
                    CanyonGame.Console.WriteLine("usage: map <mapname>");
                    return;
                }
                string newmap = argv[1];
                string[] files = Directory.GetFiles(Game.Content.RootDirectory + "/" + GameScreen.MapDirectory);
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    if (file.EndsWith(newmap + ".xnb"))
                    {
                        GameScreen gs = CanyonGame.Screens.FirstByType<GameScreen>();
                        if( gs != null )
                            gs.Changemap( newmap );
                        return;
                    }
                }
                CanyonGame.Console.WriteLine("error: couldn't find map: " + newmap);
            };
            #endregion

            #region Command 'maps'
            CanyonGame.Console.Commands["maps"] = delegate(Game game, string[] argv, GameTime gameTime)
            {
                CanyonGame.Console.WriteLine("Available maps:");
                string[] files = Directory.GetFiles(Game.Content.RootDirectory + "/" + GameScreen.MapDirectory);
                int maxlen = 0;
                List<string> maps = new List<string>();
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    file = file.Substring(0, file.IndexOf("."));
                    file = file.Substring(file.IndexOf("/") + 1);
                    file = file.Substring(file.IndexOf("\\") + 1);
                    maxlen = Math.Max(maxlen, file.Length);
                    maps.Add(file);
                }
                maxlen += 2;
                string fmt = "{0,-" + maxlen+"}";
                StringBuilder sb = new StringBuilder("  ");
                for (int i = 0; i < maps.Count; i++)
                {
                    sb.AppendFormat( fmt, maps[i] );
                    if (i % 4 == 3)
                    {
                        CanyonGame.Console.WriteLine(" " + sb.ToString());
                        sb.Clear();
                        sb.Append("  ");
                    }
                }
                CanyonGame.Console.WriteLine(" " + sb.ToString());
            };
            #endregion
        }

        public void Changemap(string newmap)
        {
            if (this.changemap == null)
                this.changemap = newmap;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (CanyonGame.Input.IsJustUp(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                CanyonGame.Instance.DoExit = true;
            if (changemap != null)
            {
                CanyonGame.Screens.Replace<GameScreen>(new GameScreen(Game, changemap));
                return;
            }

            //CanyonGame.Console.Debug(" # rockets: " + this.Components.OfType<Rocket>().Count());
            base.Update(gameTime);
        }

    }
}
