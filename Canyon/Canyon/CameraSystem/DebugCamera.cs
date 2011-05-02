using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

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
                    viewChanged = false;
                    if (ViewChanged != null)
                        ViewChanged(this);
                }
                return view;
            }
        }

        public Matrix Projection { get; protected set; }

        public event OnViewChanged ViewChanged;
        public event OnProjectionChanged ProjectionChanged;

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

        private float speed { get; set; }

        protected InputManager Input;

        public DebugCamera(Game game, Vector3 position, float horizontalRotation, float verticalRotation )
            :base(game)
        {
            Position = position;
            HorizontalRotation = horizontalRotation;
            VerticalRotation = verticalRotation;
            speed = 10;
        }

        public override void Initialize()
        {
            Input = CanyonGame.Input;
            Input.CenterMouse = true;

            orientationChanged = true;
            viewChanged = true;

            CanyonGame.Console.Commands["camera_info"] = delegate(Microsoft.Xna.Framework.Game game, string[] argv, GameTime gameTime)
            {
                CanyonGame.Console.Trace("DebugCamera: " + this.position + " (h: " + this.HorizontalRotation + " v: " + this.VerticalRotation + ")");
            };

            UpdateProjection();
            CanyonGame.Instance.GraphicsDevice.DeviceReset += delegate(object s, EventArgs e) { UpdateProjection(); };

            base.Initialize();
        }

        private void UpdateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, CanyonGame.AspectRatio,
                CanyonGame.NearPlane, CanyonGame.FarPlane);
            if (ProjectionChanged != null)
                ProjectionChanged(this);
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (CanyonGame.Camera != this)
                return;

            this.HorizontalRotation += -Input.Look.X * dt;
            this.VerticalRotation += -Input.Look.Y * dt;

            this.Position += this.Forward * -Input.Movement.Z * speed * dt;
            this.Position += this.Left * -Input.Movement.X * speed * dt;

            if (Input.IsKeyDown(Keys.LeftShift))
                this.Position += Vector3.Up * Input.Movement.Y * speed * dt;
            else
                this.Position += this.Up * Input.Movement.Y * speed * dt;

            if (Input.ScrollWheelValue != 0)
                speed = Input.ScrollWheelValue / 10;

            base.Update(gameTime);
        }


    }
}
