using Microsoft.Xna.Framework;
using Canyon.Misc;

namespace Canyon.CameraSystem
{
    public class SimpleFollowCamera : IFollowCamera
    {
        public Matrix View { get; protected set; }

        public Matrix Projection { get; protected set; }

        public event OnViewChanged ViewChanged;
        public event OnProjectionChanged ProjectionChanged;

        private Vector3 target;
        public Vector3 Target
        {
            get { return target; }
            set { target = value; UpdateView(); }
        }
        private Vector3 direciton;
        public Vector3 Direction
        {
            get { return direciton; }
            set { direciton = value; UpdateView(); }
        }

        private float distance;
        private float angle;

        public SimpleFollowCamera(float distance, float angle)
        {
            this.distance = distance;
            this.angle = angle;

            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, CanyonGame.AspectRatio,
                CanyonGame.NearPlane, CanyonGame.FarPlane);
            if (ProjectionChanged != null)
                ProjectionChanged(this);

            this.UpdateView();
        }

        private void UpdateView()
        {
            Vector3 right = Vector3.Cross(Direction, Vector3.Up);
            Vector3 position = target + (-Vector3.Transform(Direction, Matrix.CreateFromAxisAngle(right, this.angle)) * distance);
            View = Matrix.CreateLookAt(position, target, Vector3.Up);
            if (ViewChanged != null)
                ViewChanged(this);
        }
    }
}
