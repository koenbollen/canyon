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
        public Vector3 Target
        {
            get
            {
                return this.target;
            }
            set
            {
                if (this.target != value)
                {
                    this.target = value;
                    this.TargetDirectionUpChanged();
                }
            }
        }
        protected Vector3 target;

        /// <summary>
        /// The direction that the Target is facing so the camera can look from behind it.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                return this.direction;
            }
            set
            {
                if (this.direction != value)
                {
                    this.direction = value.SafeNormalize();
                    this.TargetDirectionUpChanged();
                }
            }
        }
        protected Vector3 direction;

        /// <summary>
        /// The result of the camera's LookAt position.
        /// </summary>
        public Vector3 LookAt
        {
            get
            {
                return this.lookAt;
            }
        }
        protected Vector3 lookAt;

        /// <summary>
        /// The real position of the camera at any point.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.position;
            }
        }
        protected Vector3 position;

        /// <summary>
        /// The up vector of the camera, should be set from the target.
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return this.up;
            }
            set
            {
                if (this.up != value)
                {
                    this.up = value.SafeNormalize();
                    this.TargetDirectionUpChanged();
                }
            }
        }
        protected Vector3 up;


        protected Vector3 desiredPosition;
        public Vector3 Velocity { get; protected set; }

        public SpringChaseCamera(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            this.UpdateProjection();
            Game.GraphicsDevice.DeviceReset += delegate(object s, EventArgs a)
            {
                this.UpdateProjection();
            };
            this.target = Vector3.Zero;
            this.lookAt = lookOffset;
            this.direction = Vector3.Forward;
            this.up = Vector3.Up;
            this.desiredPosition = this.position = chaseOffset;
            base.Initialize();
        }

        /// <summary>
        /// Reset the camera's motion/velocity and set the 
        /// position hard to the desired.
        /// </summary>
        public void Reset()
        {
            UpdateWorld();
            this.Velocity = Vector3.Zero;
            this.position = this.desiredPosition;
            viewChanged = true;
        }


        protected void TargetDirectionUpChanged()
        {
            UpdateWorld();
        }

        /// <summary>
        /// When the target, direction or up changes this method updates
        /// the desiredPosition and lookAt.
        /// </summary>
        protected void UpdateWorld()
        {
            Matrix transform = Matrix.Identity;
            Vector3 right = Vector3.Cross(this.up, this.direction);
            transform.Forward = this.direction;
            transform.Up = this.up;
            transform.Right = right;

            desiredPosition = Target + Vector3.Transform(chaseOffset, transform);
            lookAt = Target + Vector3.Transform(lookOffset, transform);
        }

        /// <summary>
        /// Update/Step. Set the real position one step closer to the desired position.
        /// </summary>
        /// <param name="gameTime">Step size.</param>
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 stretch = position - desiredPosition;
            Vector3 force = -Stiffness * stretch - Damping * Velocity;

            Vector3 acceleration = force / Mass;
            Velocity += acceleration * dt;

            position += Velocity * dt;

            viewChanged = true;

            base.Update(gameTime);
        }

        /// <summary>
        /// Update the View matrix using this.position, this.lookAt and this.up.
        /// </summary>
        protected void UpdateView()
        {
            viewChanged = false;
            view = Matrix.CreateLookAt(this.position, this.lookAt, this.up);
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
    }
}
