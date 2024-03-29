using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;


namespace Canyon.Screens
{
    /// <summary>
    /// This class is a collection/stack of active screens in the game.
    /// A game should have one screen manager and control the active screens by 
    /// adding and removing screens from this manager.
    /// 
    /// The top screen of the stack will be updated and the top screens that are 
    /// translucent will be drawn. The first screen on the stack that isn't translucent
    /// will stop the drawing.
    /// 
    /// In Game:
    /// public ScreenManager Screens { get; private set; }
    /// 
    /// In Game.Initialize():
    /// this.Components.Add( this.Screens = new ScreenManager(this, new StartScreen(this)) );
    /// 
    /// </summary>
    /// By Koen Bollen, 2011
    public class ScreenManager : DrawableGameComponent
    {
        /// <summary>
        /// This boolean is set when the Initialize() method is called.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// This is the list of active screens in the game.
        /// </summary>
        private Stack<Screen> screens;

        /// <summary>
        /// The current active screen.
        /// </summary>
        public Screen ActiveScreen
        {
            get { return this.Peek(); }
        }

        private Screen startScreen;

        public ScreenManager(Game game, Screen start)
            : base(game)
        {
            this.screens = new Stack<Screen>();
            startScreen = start;
            this.Initialized = false;
        }

        /// <summary>
        /// Initialize all screens in the active screen list.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            CanyonGame.Console.Trace("ScreenManager initialized.");
            if (startScreen != null)
                this.Push(startScreen);
            foreach (Screen s in this.screens)
                s.Initialize();
            this.Initialized = true;
        }

        /// <summary>
        /// Only update the top of the screen stack.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (this.ActiveScreen != null)
            {
                this.ActiveScreen.Update(gameTime);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw all visible screens. A screen that is not translucent will stop the iteration of screens.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            List<Screen> visible = new List<Screen>();
            foreach (Screen s in this.screens)
            {
                visible.Add(s);
                if (!s.Translucent)
                    break;
            }

            // Draw from back to front:
            for( int i = visible.Count-1; i >= 0; i-- )
                visible[i].Draw(gameTime);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Add a screen to the top of the stack, if the manager is initialized but the screen isn't initialize it.
        /// Also set it's manager to this.
        /// </summary>
        /// <param name="screen">The screen to add.</param>
        public void Push(Screen screen)
        {
            screen.Manager = this;
            if (!screen.Initialized && this.Initialized)
                screen.Initialize();
            if( this.ActiveScreen != null )
                this.ActiveScreen.Deactivated();
            this.screens.Push(screen);
            CanyonGame.Console.Trace("Screen changed to " + ActiveScreen.GetType().Name);
        }

        /// <summary>
        /// Get the top of the screen stack. The most active screen.
        /// </summary>
        /// <returns>The active screen.</returns>
        public Screen Peek()
        {
            if (this.screens.Count < 1 )
                return null;
            return this.screens.Peek();
        }

        /// <summary>
        /// Remove a screen from the screen stack.
        /// </summary>
        /// <returns>The removed screen.</returns>
        public Screen Pop()
        {
            if (this.screens.Count < 1)
                return null;
            Screen prev = this.screens.Pop();
            if (this.ActiveScreen != null)
                this.ActiveScreen.Activated();
            CanyonGame.Console.Trace("Screen changed to " + ActiveScreen.GetType().Name);
            return prev;
        }

        /// <summary>
        /// This method finds the first Screen of a specific 
        /// type and replaces it with a new one.
        /// 
        /// It searches from the top of the stack and does 
        /// the popping it self. The ManagerMethod it uses
        /// is the Push() method when it finds the screen 
        /// to replace.
        /// </summary>
        /// <typeparam name="T">Type of Screen to replace.</typeparam>
        /// <param name="newScreen">The new screen to replace the found screen of T.</param>
        /// <returns></returns>
        public Screen Replace<T>(Screen newScreen) where T : Screen
        {
            if (this.screens.Count < 1)
                return null;

            Screen prev = null;
            Stack<Screen> store = new Stack<Screen>();
            while (this.screens.Count > 0 && !(this.screens.Peek() is T) )
            {
                store.Push(this.screens.Pop());
            }
            if (this.screens.Count > 0 && (this.screens.Peek() is T))
            {
                prev = this.screens.Pop();
                prev.Deactivated();
                this.Push(newScreen);
            }
            while( store.Count > 0 )
                this.screens.Push(store.Pop());
            return null;
        }

        /// <summary>
        /// Return the first of a certain screen type. Used 
        /// the normal Enumerator to loop through the stack.
        /// </summary>
        /// <typeparam name="T">The type to find.</typeparam>
        /// <returns>The found Screen of type T or null if none where found.</returns>
        public T FirstByType<T>() where T : Screen
        {
            foreach (Screen s in this.screens)
                if (s is T)
                    return s as T;
            return null;
        }
    }
}
