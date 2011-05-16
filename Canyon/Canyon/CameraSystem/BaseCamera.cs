using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    public class BaseCamera : ICamera
    {
        public Matrix View { get; protected set; }

        public Matrix Projection { get; protected set; }

        public event OnProjectionChanged ProjectionChanged;

        /// <summary>
        /// Initialize the View and Projection matrices to Matrix.Identity.
        /// </summary>
        public BaseCamera()
            :this(Matrix.Identity, Matrix.Identity)
        {
        }

        /// <summary>
        /// Simply set the matrices.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="projection">The bad.</param>
        public BaseCamera(Matrix view, Matrix projection)
        {
            this.View = view;
            this.Projection = projection;
            if (ProjectionChanged != null)
                ProjectionChanged(this);
        }

        /// <summary>
        /// Create a simple static camera using the methods Matrix.CreateLookAt and Matrix.CreatePerspectiveFieldOfView.
        /// </summary>
        /// <param name="position">World location of this camera.</param>
        /// <param name="target">The target point of the camera (not the direction).</param>
        /// <param name="aspectRatio">The aspect ratio of the render result (commenly GraphicsDevice.Viewport.AspectRatio).</param>
        /// <param name="nearPlaneDistance">Near plane of this view.</param>
        /// <param name="farPlaneDistance">Far plane of this view.</param>
        public BaseCamera(Vector3 position, Vector3 target, Vector3 up)
            :this(
                Matrix.CreateLookAt(position, target, up), 
                Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, CanyonGame.AspectRatio,
                    CanyonGame.NearPlane, CanyonGame.FarPlane)
            )
        {
        }
    }
}
