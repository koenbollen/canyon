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

        void HardSet(IFollowable target=null);
        IFollowable GetStateAsTarget();
    }
}
