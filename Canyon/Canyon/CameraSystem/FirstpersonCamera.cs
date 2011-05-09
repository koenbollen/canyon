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

        public event OnViewChanged ViewChanged;
        public event OnProjectionChanged ProjectionChanged;

        protected LerpVector3 target = new LerpVector3(1, 1);
        public Vector3 Target
        {
            get { return target.Value; }
            set { target.Value = value; }
        }

        protected LerpVector3 direction = new LerpVector3(1, 1);
        public Vector3 Direction
        {
            get { return direction.Value; }
            set { direction.Value = value; }
        }

        protected LerpVector3 up = new LerpVector3(1, 1);
        public Vector3 Up {
            get { return up.Value; }
            set { up.Value = value; }
        }

        public FirstpersonCamera(Game game)
            :base(game)
        {
            UpdateProjection();
            game.GraphicsDevice.DeviceReset += delegate(object s, EventArgs a)
            {
                UpdateProjection();
            };
            Enabled = false;
        }

        public void Reset()
        {
            target.Reset();
            direction.Reset();
            up.Reset();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            target.Step(dt);
            direction.Step(dt);
            up.Step(dt);
            //CanyonGame.Console.Debug(target.ToString());
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
            View = Matrix.CreateLookAt(this.Target, this.Target + this.Direction, this.Up);
            if (ViewChanged != null)
                ViewChanged.Invoke(this);
        }
    }
}
