using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Canyon.Misc;

namespace Canyon.CameraSystem
{
    public class FirstpersonCamera : GameComponent, IFollowCamera
    {
        public const float Delay     = .8f;
        public const float Threshold = 1.0f;

        public Matrix View { get { return CreateView(); } }
        public Matrix Projection { get; protected set; }

        public event OnProjectionChanged ProjectionChanged;

        public IFollowable Target { get; set; }

        private Followable start;
        private Followable real;

        private float time;

        public FirstpersonCamera(Game game, IFollowable target=null)
            :base(game)
        {
            this.Target = target;
            this.start = this.real = null;

            time = 0;
            Enabled = false;

            UpdateProjection();
            game.GraphicsDevice.DeviceReset += delegate(object s, EventArgs a)
            {
                UpdateProjection();
            };
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (time > 0)
            {
                this.real.Position = Vector3.Lerp(this.Target.Position, this.start.Position, time / FirstpersonCamera.Delay);
                this.real.Orientation = Quaternion.Slerp(this.Target.Orientation, this.start.Orientation, time / FirstpersonCamera.Delay);
                time -= dt;
            }
            else
            {
                time = 0;
                this.start = this.real = null;
                this.Enabled = false;
            }

            base.Update(gameTime);
        }

        private void UpdateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, CanyonGame.AspectRatio, CanyonGame.NearPlane, CanyonGame.FarPlane);
            if (ProjectionChanged != null)
                ProjectionChanged.Invoke(this);
        }

        private Matrix CreateView()
        {
            if (Enabled && this.real != null)
                return Matrix.CreateLookAt(this.real.Position, this.real.Position + this.real.Orientation.Forward(), this.real.Orientation.Up());
            return Matrix.CreateLookAt(this.Target.Position, this.Target.Position + this.Target.Orientation.Forward(), this.Target.Orientation.Up());
        }

        public void HardSet(IFollowable target = null)
        {
            if (target != null)
            {
                start = new Followable(target);
                real = new Followable(target);
                this.time = FirstpersonCamera.Delay;
                this.Enabled = true;
            }
            else
            {
                time = 0;
                this.start = this.real = null;
                this.Enabled = false;
            }
        }

        public IFollowable GetCurrentStateAsTarget()
        {
            if (Enabled)
                return real;
            else
                return this.Target;
        }
    }
}
