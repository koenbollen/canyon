using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    public delegate void OnCameraChanged( Matrix view, Matrix projection );

    /// <summary>
    /// The very core of a 3D camera. Object implementing this interface have the two matrices needed to render.
    /// </summary>
    public interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }

        event OnCameraChanged CameraChanged;
    }
}
