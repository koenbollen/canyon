using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Canyon.CameraSystem
{
    /// <summary>
    /// A simple free camera that is controlled by the mouse and keyboard.
    /// 
    /// TODO: Use an inputmanager of some sorts.
    /// </summary>
    public class DebugCamera : GameComponent, ICamera
    {
        private bool viewChanged;
        private Matrix view;
        public Matrix View
        {
            get 
            {
                if (viewChanged)
                {
                    view = Matrix.CreateLookAt(this.Position, this.Position + this.Forward, this.Up);
                    if (CameraChanged != null)
                        CameraChanged(view, Projection);
                    viewChanged = true;
                }
                return view;
            }
        }

        public Matrix Projection { get; protected set; }

        public event OnCameraChanged CameraChanged;

        private Vector3 position;
        public Vector3 Position { get { return position; } set { position = value; viewChanged = true; } }

        private float horizontal;
        public float HorizontalRotation
        {
            get { return horizontal; }
            set { if (value != horizontal) { horizontal = value; orientationChanged = true; } }
        }
        private float vertical;
        public float VerticalRotation
        {
            get { return vertical; }
            set { if (value != vertical) { vertical = value; orientationChanged = true; } }
        }

        private bool orientationChanged;
        private Quaternion orientation;
        public Quaternion Orientation
        {
            get
            {
                if (orientationChanged)
                {
                    orientation = Quaternion.CreateFromYawPitchRoll(horizontal, vertical, 0);
                    orientationChanged = false;
                    viewChanged = true;
                }
                return orientation;
            }
        }

        public Vector3 Forward { get { return Vector3.Transform(Vector3.Forward, this.Orientation); } }
        public Vector3 Left { get { return Vector3.Transform(Vector3.Left, this.Orientation); } }
        public Vector3 Up { get { return Vector3.Transform(Vector3.Up, this.Orientation); } }

        public float MoveFactor { get; set; }

        public DebugCamera(Game game, Vector3 position, float horizontalRotation, float verticalRotation, float aspectRatio, float nearPlane, float farPlane )
            :base(game)
        {
            MoveFactor = 10;
            Position = position;
            HorizontalRotation = horizontalRotation;
            VerticalRotation = verticalRotation;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearPlane, farPlane);
        }

        public override void Initialize()
        {
            orientationChanged = true;
            viewChanged = true;

            Mouse.SetPosition(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);

            CanyonGame.Console.Commands["camera"] = delegate(Microsoft.Xna.Framework.Game game, string[] argv, GameTime gameTime)
            {
                CanyonGame.Console.Trace("DebugCamera: " + this.position + " (h: " + this.HorizontalRotation + " v: " + this.VerticalRotation + ")");
            };

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 center = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);

            MouseState mouseState = Mouse.GetState();
            KeyboardState kbs = Keyboard.GetState();

            float dx = mouseState.X - center.X;
            float dy = mouseState.Y - center.Y;

            float screenFactorX = (500.0f * Game.GraphicsDevice.Viewport.AspectRatio) / Game.GraphicsDevice.Viewport.Width;
            float screenFactorY = 500.0f / Game.GraphicsDevice.Viewport.Height;

            this.HorizontalRotation += -dx * dt * screenFactorX;
            this.VerticalRotation += -dy * dt * screenFactorY;

            if (kbs.IsKeyDown(Keys.W))
                this.Position += this.Forward * MoveFactor * dt;
            if (kbs.IsKeyDown(Keys.S))
                this.Position += -this.Forward * MoveFactor * dt;
            if (kbs.IsKeyDown(Keys.A))
                this.Position += this.Left * MoveFactor * dt;
            if (kbs.IsKeyDown(Keys.D))
                this.Position += -this.Left * MoveFactor * dt;

            if (kbs.IsKeyDown(Keys.Space) && kbs.IsKeyDown(Keys.LeftShift))
                this.Position += Vector3.Up * MoveFactor * dt;
            else if (kbs.IsKeyDown(Keys.Space))
                this.Position += this.Up * MoveFactor * dt;

            if (mouseState.ScrollWheelValue != 0)
                MoveFactor = mouseState.ScrollWheelValue / 10;

            Mouse.SetPosition((int)center.X, (int)center.Y);
            base.Update(gameTime);
        }


    }
}
