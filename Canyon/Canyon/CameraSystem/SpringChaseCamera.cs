using System;
using Canyon.Misc;
using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    public class SpringChaseCamera : GameComponent, IFollowCamera
    {
        public const float Mass = 50.0f;
        public const float Stiffness = 1800.0f;
        public const float Damping = 600.0f;
        public static readonly Vector3 chaseOffset = new Vector3(0, 1, 2);
        public static readonly Vector3 lookOffset = new Vector3(0, .28f, -.5f);

        protected bool viewChanged;
        protected Matrix view;
        public Matrix View
        {
            get
            {
                if (viewChanged)
                {
                    UpdateView();
                    viewChanged = false;
                }
                return view;
            }
        }
        public Matrix Projection { get; protected set; }

        public event OnProjectionChanged ProjectionChanged;

        /// <summary>
        /// The target of the follow camera, the camera will look at this point + the lookOffset.
        /// </summary>
        public IFollowable Target { get; set; } 

        /// <summary>
        /// The result of the camera's LookAt position.
        /// </summary>
        public Vector3 LookAt { get; protected set; }

        /// <summary>
        /// The real position of the camera at any point.
        /// </summary>
        public Vector3 Position { get; protected set; }

        protected Vector3 desiredPosition;
        public Vector3 Velocity { get; protected set; }

        public SpringChaseCamera(Game game, IFollowable target = null)
            : base(game)
        {
            this.Target = target;
        }

        public override void Initialize()
        {
            this.UpdateProjection();
            Game.GraphicsDevice.DeviceReset += delegate(object s, EventArgs a)
            {
                this.UpdateProjection();
            };
            this.LookAt = lookOffset;
            this.desiredPosition = this.Position = chaseOffset;
            base.Initialize();
        }

        /// <summary>
        /// Reset the camera's motion/velocity and set the 
        /// position hard to the desired.
        /// </summary>
        public void HardSet(IFollowable target=null)
        {
            if (target != null)
            {
                IFollowable old = this.Target;
                this.Target = target;
                UpdateWorld();
                this.Target = old;
            }
            else
            {
                UpdateWorld();
            }
            this.Velocity = Vector3.Zero;
            this.Position = this.desiredPosition;
            viewChanged = true;
        }

        /// <summary>
        /// When the target, direction or up changes this method updates
        /// the desiredPosition and lookAt.
        /// </summary>
        protected void UpdateWorld()
        {
            desiredPosition = this.Target.Position + Vector3.Transform(chaseOffset, this.Target.Orientation);
            LookAt = this.Target.Position + Vector3.Transform(lookOffset, this.Target.Orientation);
        }

        /// <summary>
        /// Update/Step. Set the real position one step closer to the desired position.
        /// </summary>
        /// <param name="gameTime">Step size.</param>
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateWorld();

            Vector3 stretch = Position - desiredPosition;
            Vector3 force = -Stiffness * stretch - Damping * Velocity;

            Vector3 acceleration = force / Mass;
            Velocity += acceleration * dt;

            Position += Velocity * dt;

            viewChanged = true;

            base.Update(gameTime);
        }

        /// <summary>
        /// Update the View matrix using this.position, this.lookAt and this.up.
        /// </summary>
        protected void UpdateView()
        {
            viewChanged = false;
            view = Matrix.CreateLookAt(this.Position, this.LookAt, this.Target.Orientation.Up());
        }

        /// <summary>
        /// Update the projection if  the aspectratio changes.
        /// </summary>
        protected void UpdateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, CanyonGame.AspectRatio, CanyonGame.NearPlane, CanyonGame.FarPlane);
            if (this.ProjectionChanged != null)
                this.ProjectionChanged.Invoke(this);
        }

        public IFollowable GetCurrentStateAsTarget()
        {
            Vector3 up = this.Target.Orientation.Up();
            Vector3 forward = (this.LookAt-this.Position).SafeNormalize();
            Vector3 right = Vector3.Cross(forward, up);
            up = Vector3.Cross(right, forward);

            Matrix rotation = Matrix.Identity;
            rotation.Forward = forward;
            rotation.Up = up;
            rotation.Right = right;
            return new Followable(this.Position, Quaternion.CreateFromRotationMatrix(rotation));
        }

    }
}
