using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    /// <summary>
    /// Instances implementing this interface can be followed by IFollowCameras.
    /// </summary>
    public interface IFollowable
    {
        Vector3 Position { get; }
        Quaternion Orientation { get; }
    }

    /// <summary>
    /// Basic copy class for IFollowable.
    /// </summary>
    public class Followable : IFollowable
    {
        public Vector3 Position { get; set;  }
        public Quaternion Orientation { get; set; }
        public Followable(Vector3 position, Quaternion orientation)
        {
            this.Position = position;
            this.Orientation = orientation;
        }
        public Followable(IFollowable followable)
        {
            this.Position = followable.Position;
            this.Orientation = followable.Orientation;
        }
    }
}
