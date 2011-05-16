using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    public interface IFollowCamera : ICamera
    {
        IFollowable Target { get; set; }

        //Vector3 Position { get; }

        void HardSet(IFollowable target=null);
        IFollowable GetCurrentStateAsTarget();
    }
}
