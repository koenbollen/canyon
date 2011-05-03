using Microsoft.Xna.Framework;

namespace Canyon.Entities
{
    public interface IEntity
    {
        Vector3 Position  { get; }
        Quaternion Orientation { get; }
    }
}
