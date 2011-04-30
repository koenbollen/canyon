using Microsoft.Xna.Framework;
using System.Linq;


namespace Canyon.Screens
{
    /// <summary>
    /// This is a screen that can be added to the ScreenManager. Extend it and add components 
    /// to it in the Initialize() method. You can also override the Update() and Draw() method.
    /// </summary>
    public class Screen : DrawableGameComponent
    {
        /// <summary>
        /// This member tells if this screen is initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Set this member to true if this screen doesn't cover the entire screen.
        /// </summary>
        public bool Translucent { get; set; }

        /// <summary>
        /// A reference to the ScreenManager.
        /// </summary>
        public ScreenManager Manager { get; internal set; }

        /// <summary>
        /// The GameComponentCollection, add components for this screen.
        /// </summary>
        public GameComponentCollection Components { get; private set; }

        public Screen(Game game)
            : base(game)
        {
            this.Components = new GameComponentCollection();
            Translucent = false;
        }

        /// <summary>
        /// This method is called when this screen is back at the top of the stack.
        /// </summary>
        public virtual void Activated()
        {
        }

        /// <summary>
        /// This method is called when a screen is deactivated by an other screen.
        /// </summary>
        public virtual void Deactivated()
        {
        }

        /// <summary>
        /// Initialize every Component of this screen.
        /// </summary>
        public override void Initialize()
        {
            foreach (GameComponent gc in this.Components)
                gc.Initialize();
            Initialized = true;
            base.Initialize();
        }

        /// <summary>
        /// Update every Enabled Component of this screen.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Major credits to Nils Dijk:
            foreach (IUpdateable gc in this.Components.OfType<IUpdateable>().Where<IUpdateable>(x => x.Enabled).OrderBy<IUpdateable, int>(x => x.UpdateOrder))
                gc.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// Draw every Visible Component of this screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Major credits to Nils Dijk:
            foreach (IDrawable gc in this.Components.OfType<IDrawable>().Where<IDrawable>(x => x.Visible).OrderBy<IDrawable,int>(x => x.DrawOrder) )
                    gc.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
