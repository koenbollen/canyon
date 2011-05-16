using Microsoft.Xna.Framework;
using Canyon.Misc;
using System;

namespace Canyon.CameraSystem
{
    public class SimpleFollowCamera : IFollowCamera
    {
        public Matrix View
        {
            get
            {
                return CreateView();
            }
        }

        public Matrix Projection { get; protected set; }

        public event OnProjectionChanged ProjectionChanged;

        public IFollowable Target { get; set; }

        private float distance;
        private float angle;

        public SimpleFollowCamera(IFollowable target, float distance, float angle)
        {
            this.Target = target;
            this.distance = distance;
            this.angle = angle;

            UpdateProjection();
            CanyonGame.Instance.GraphicsDevice.DeviceReset += delegate(object s, EventArgs e) { UpdateProjection(); };

            this.CreateView();
        }

        public void Reset()
        {
        }

        private void UpdateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, CanyonGame.AspectRatio,
                CanyonGame.NearPlane, CanyonGame.FarPlane);
            if (ProjectionChanged != null)
                ProjectionChanged(this);
        }

        private Matrix CreateView()
        {
            if (this.Target == null)
                return Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
            Vector3 right = this.Target.Orientation.Right();
            Vector3 direction = this.Target.Orientation.Forward();
            Vector3 position = this.Target.Position + (-Vector3.Transform(direction, Matrix.CreateFromAxisAngle(right, this.angle)) * distance);
            return Matrix.CreateLookAt(position, this.Target.Position, this.Target.Orientation.Up());
        }

        public void HardSet(IFollowable target=null)
        {
            // This is already a hard follow camera.
        }

        public IFollowable GetCurrentStateAsTarget()
        {
            return this.Target;
        }

    }
}
