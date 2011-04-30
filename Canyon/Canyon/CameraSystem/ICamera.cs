using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    /// <summary>
    /// The very core of a 3d camera. Object implementing this interface have the two matrices needed to render.
    /// </summary>
    public interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }
    }
}
