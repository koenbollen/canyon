using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    public interface IFollowCamera : ICamera
    {
        Vector3 Target { get; set; }
        Vector3 Direction { get; set; }
        Vector3 Up { get; set; }

        void Reset();
    }
}
