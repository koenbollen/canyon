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
        public Matrix View { get; protected set; }
        public Matrix Projection { get; protected set; }

        public event OnProjectionChanged ProjectionChanged;

        public IFollowable Target { get; set; }

        public FirstpersonCamera(Game game, IFollowable target=null)
            :base(game)
        {
            this.Target = target;
            UpdateProjection();
            game.GraphicsDevice.DeviceReset += delegate(object s, EventArgs a)
            {
                UpdateProjection();
            };
            Enabled = false;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateView();
            base.Update(gameTime);
        }

        private void UpdateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, CanyonGame.AspectRatio, CanyonGame.NearPlane, CanyonGame.FarPlane);
            if (ProjectionChanged != null)
                ProjectionChanged.Invoke(this);
        }

        private void UpdateView()
        {
            Vector3 direction = this.Target.Orientation.Forward();
            Vector3 up = this.Target.Orientation.Up();
            View = Matrix.CreateLookAt(this.Target.Position, this.Target.Position + direction, up);
        }

        public void HardSet(IFollowable target = null)
        {
            if (target != null)
            {
            }
        }

        public IFollowable GetStateAsTarget()
        {
            return this.Target;
        }
    }
}
