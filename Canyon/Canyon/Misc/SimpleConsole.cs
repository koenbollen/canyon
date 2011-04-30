using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Canyon.Misc
{
    public delegate void ConsoleAction( Game game, string[] argv, GameTime gameTime );

    public class ConsoleSettings
    {
        public Color Background = Color.Black;
        public Color Color = Color.White;
        public float Alpha = 0.8f;
        public int LineCount = 10;
        public float Padding = 4;
        public string FontAsset = "Fonts/console";
    }

    public class SimpleConsole : DrawableGameComponent
    {

        private ConsoleSettings Settings;

        public Dictionary<string, ConsoleAction> Commands { get; protected set; }

        private List<string> lines;
        private string input;
        private int cursor;
        private string last;

        private float height { get { if (this.font == null) return 200; return (this.font.LineSpacing * Settings.LineCount) + (2*Settings.Padding); } }

        private BasicEffect effect;
        private SpriteBatch batch;
        private SpriteFont font;

        private KeyboardState kbs;
        private KeyMap keyMap;

        public SimpleConsole(Game game, ConsoleSettings settings = null )
            : base(game)
        {
            this.Settings = settings;
            if (this.Settings == null)
                this.Settings = new ConsoleSettings();

            this.Commands = new Dictionary<string, ConsoleAction>();
            this.lines = new List<string>();
            this.lines.Add("] "+Game.GetType().Name + " console");
            this.input = this.last = "";

            this.DrawOrder = int.MaxValue / 2;
            this.UpdateOrder = int.MaxValue / 2;
        } 

        public void RegisterDefaults()
        {
            this.Commands["quit"] = delegate(Game game, string[] argv, GameTime gameTime)
            {
                game.Exit();
            };
            this.Commands["echo"] = delegate(Game game, string[] argv, GameTime gameTime)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < argv.Length; i++)
                {
                    sb.Append(argv[i]);
                    if (i != argv.Length - 1)
                        sb.Append(" ");
                }
                this.lines.Add(sb.ToString());
            };
            this.Commands["commands"] = delegate(Game game, string[] argv, GameTime gameTime)
            {
                this.lines.Add("available commands:");
                this.lines.Add(this.Commands.Keys.ToString());
            };

            // aliases:
            this.Commands["exit"] = this.Commands["quit"];
            this.Commands["print"] = this.Commands["echo"];
            this.Commands["help"] = this.Commands["commands"];
        }

        public override void Initialize()
        {
            this.Visible = false;
            this.RegisterDefaults();
            this.kbs = Keyboard.GetState();
            this.keyMap = new KeyMap();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = new BasicEffect(Game.GraphicsDevice);
            batch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>(Settings.FontAsset);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState kbs = Keyboard.GetState();
            if (!this.Visible)
            {
                if (kbs.IsKeyDown(Keys.OemTilde) && this.kbs.IsKeyUp(Keys.OemTilde))
                    this.Visible = true;
                this.kbs = kbs;
                base.Update(gameTime);
                return;
            }
            if (kbs.IsKeyDown(Keys.Escape) && this.kbs.IsKeyUp(Keys.Escape))
            {
                this.Visible = false;
                this.kbs = kbs;
                base.Update(gameTime);
                return;
            }

            #region Read typed letters:
            bool shift = kbs.IsKeyDown(Keys.LeftShift) || kbs.IsKeyDown(Keys.RightShift);
            foreach (Keys k in kbs.GetPressedKeys())
            {
                if (!this.kbs.IsKeyUp(k))
                    continue;
                char c = this.keyMap.getChar(k,shift?KeyMap.Modifier.Shift:KeyMap.Modifier.None);
                if (c != '\0')
                    this.input = this.input.Insert(this.cursor++, c.ToString());
            }
            #endregion

            #region Cursor navigation:
            if (kbs.IsKeyDown(Keys.Left) && this.kbs.IsKeyUp(Keys.Left))
                this.cursor--;
            if (kbs.IsKeyDown(Keys.Right) && this.kbs.IsKeyUp(Keys.Right))
                this.cursor++;
            this.cursor = (int)MathHelper.Clamp(this.cursor, 0, this.input.Length);
            if (kbs.IsKeyDown(Keys.Back) && this.kbs.IsKeyUp(Keys.Back) && this.cursor > 0)
            {
                this.input = this.input.Remove(this.cursor - 1, 1);
                this.cursor = (int)MathHelper.Clamp(this.cursor - 1, 0, this.input.Length);
            }
            #endregion

            #region Handle command execution:
            if (kbs.IsKeyDown(Keys.Enter) && this.kbs.IsKeyUp(Keys.Enter ) )
            {
                string command;
                if (this.input.Length > 0)
                    command = this.last = this.input;
                else
                    command = this.last;
                if (command.Length > 0)
                {
                    string[] argv = command.Trim().Split();
                    if (this.Commands.ContainsKey(argv[0]))
                        this.Commands[argv[0]](Game, argv, gameTime);
                    else
                        this.lines.Add("command not found: " + argv[0]);
                }
                this.input = "";
                this.cursor = 0;
            }
            #endregion

            this.kbs = kbs;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            string inputline = "] " + this.input;
            float y = this.height - Settings.Padding - this.font.LineSpacing;

            #region Draw Background Quad:
            VertexPositionColor[] vertices = new VertexPositionColor[] {
                new VertexPositionColor( new Vector3(0, 0, 0), Color.White ),
                new VertexPositionColor( new Vector3(Game.GraphicsDevice.Viewport.Width, 0, 0), Color.White ),
                new VertexPositionColor( new Vector3(Game.GraphicsDevice.Viewport.Width, this.height, 0), Color.White ),
                    
                new VertexPositionColor( new Vector3(0, 0, 0), Color.White ),
                new VertexPositionColor( new Vector3(Game.GraphicsDevice.Viewport.Width, this.height, 0), Color.White ),
                new VertexPositionColor( new Vector3(0, height, 0), Color.White )
            };

            this.effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, 0,
                0, 1);
            this.effect.DiffuseColor = this.Settings.Background.ToVector3();
            this.effect.Alpha = this.Settings.Alpha;

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, 2);
            }
            #endregion

            this.batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);


            #region Draw input and lines:
            this.batch.DrawString(this.font, inputline, new Vector2(Settings.Padding, y), this.Settings.Color);
            y -= this.font.LineSpacing;

            int index = 1;
            while (index < Settings.LineCount && this.lines.Count-index >= 0)
            {
                string line = this.lines[this.lines.Count - index];
                foreach( string chunk in BuildLines(Game.GraphicsDevice.Viewport.Width - Settings.Padding*2, line ) )
                {
                    this.batch.DrawString(this.font, chunk, new Vector2(Settings.Padding, y), this.Settings.Color);
                    y -= this.font.LineSpacing;
                }
                index++;
            }
            #endregion

            this.batch.End();

            #region Draw cursor:
            Vector2 cursor = this.font.MeasureString(inputline.Substring(0, 2 + this.cursor)) + new Vector2(Settings.Padding, 0);
            cursor.Y = height - this.font.LineSpacing - Settings.Padding;
            effect.DiffuseColor = Settings.Color.ToVector3();
            effect.Alpha = 1.0f;

            VertexPositionColor[] cursorverts = new VertexPositionColor[] {
                        new VertexPositionColor( new Vector3(cursor, 0), Color.White ),
                        new VertexPositionColor( new Vector3(cursor.X, cursor.Y + font.LineSpacing, 0), Color.White )
                    };
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList,
                    cursorverts, 0, 1);
            }
            #endregion

            base.Draw(gameTime);
        }

        public void WriteLine(string line)
        {
            this.lines.Add(line);
        }
        public void Trace(string line)
        {
#if TRACE
            this.lines.Add( "trace: " + line);
#endif // TRACE
        }
        public void Debug(string line)
        {
#if DEBUG
            this.lines.Add("debug: " + line);
#endif // DEBUG
        }

        private List<string> BuildLines(float width_bound, string text)
        {
            List<string> result = new List<string>();
            StringBuilder line = new StringBuilder();

            float width = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                Vector2 size = this.font.MeasureString(c.ToString());
                if (width + size.X >= width_bound)
                {
                    result.Insert(0, line.ToString());
                    line.Remove(0, line.Length);
                    line.Append(c);
                    width = size.X;
                }
                else
                {
                    line.Append(text[i]);
                    width += size.X;
                }
            }

            if (line.Length > 0)
                result.Insert(0, line.ToString());

            return result;
        }
    }


    public class KeyMap
    {
        public enum Modifier : int
        {
            None,
            Shift,
        }

        private Dictionary<Keys, Dictionary<Modifier, char>> map;

        public KeyMap()
        {
            map = new Dictionary<Keys, Dictionary<Modifier, char>>();
            map[Keys.Space] = new Dictionary<Modifier, char>();
            map[Keys.Space][Modifier.None] = ' ';

            char[] specials = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };

            for (int i = 0; i <= 9; i++)
            {
                char c = (char)(i + 48);
                map[(Keys)c] = new Dictionary<Modifier, char>();
                map[(Keys)c][Modifier.None] = c;
                map[(Keys)c][Modifier.Shift] = specials[i];
            }

            for (char c = 'A'; c <= 'Z'; c++)
            {
                map[(Keys)c] = new Dictionary<Modifier, char>();
                map[(Keys)c][Modifier.None] = (char)(c + 32);
                map[(Keys)c][Modifier.Shift] = c;
            }

            map[Keys.OemPipe] = new Dictionary<Modifier, char>();
            map[Keys.OemPipe][Modifier.None] = '\\';
            map[Keys.OemPipe][Modifier.Shift] = '|';

            map[Keys.OemOpenBrackets] = new Dictionary<Modifier, char>();
            map[Keys.OemOpenBrackets][Modifier.None] = '[';
            map[Keys.OemOpenBrackets][Modifier.Shift] = '{';

            map[Keys.OemCloseBrackets] = new Dictionary<Modifier, char>();
            map[Keys.OemCloseBrackets][Modifier.None] = ']';
            map[Keys.OemCloseBrackets][Modifier.Shift] = '}';

            map[Keys.OemComma] = new Dictionary<Modifier, char>();
            map[Keys.OemComma][Modifier.None] = ',';
            map[Keys.OemComma][Modifier.Shift] = '<';

            map[Keys.OemPeriod] = new Dictionary<Modifier, char>();
            map[Keys.OemPeriod][Modifier.None] = '.';
            map[Keys.OemPeriod][Modifier.Shift] = '>';

            map[Keys.OemSemicolon] = new Dictionary<Modifier, char>();
            map[Keys.OemSemicolon][Modifier.None] = ';';
            map[Keys.OemSemicolon][Modifier.Shift] = ':';

            map[Keys.OemQuestion] = new Dictionary<Modifier, char>();
            map[Keys.OemQuestion][Modifier.None] = '/';
            map[Keys.OemQuestion][Modifier.Shift] = '?';

            map[Keys.OemQuotes] = new Dictionary<Modifier, char>();
            map[Keys.OemQuotes][Modifier.None] = '\'';
            map[Keys.OemQuotes][Modifier.Shift] = '"';

            map[Keys.OemMinus] = new Dictionary<Modifier, char>();
            map[Keys.OemMinus][Modifier.None] = '-';
            map[Keys.OemMinus][Modifier.Shift] = '_';

            map[Keys.OemPlus] = new Dictionary<Modifier, char>();
            map[Keys.OemPlus][Modifier.None] = '=';
            map[Keys.OemPlus][Modifier.Shift] = '+';
        }

        public char getChar(Keys key, Modifier mod)
        {
            if (!map.ContainsKey(key))
                return '\0';
            if (!map[key].ContainsKey(mod))
                return '\0';
            return map[key][mod];
        }

        public List<char> listChars()
        {
            List<char> chars = new List<char>();

            foreach (Keys key in map.Keys)
                foreach (Modifier mod in map[key].Keys)
                    if (!chars.Contains(map[key][mod]))
                        chars.Add(map[key][mod]);

            return chars;
        }
    }
}
