using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    public interface IFollowable
    {
        Vector3 Position { get; }
        Quaternion Orientation { get; }
    }

    public class Followable : IFollowable
    {
        public Vector3 Position { get; protected set;  }
        public Quaternion Orientation { get; protected set; }
        public Followable(Vector3 position, Quaternion orientation)
        {
            this.Position = position;
            this.Orientation = orientation;
        }
    }
}
