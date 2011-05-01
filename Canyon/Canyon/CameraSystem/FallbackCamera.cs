﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Canyon.CameraSystem
{
    public class FallbackCamera : ICamera
    {
        public Matrix View { get; protected set; }
        public Matrix Projection { get; protected set; }

        public event OnViewChanged ViewChanged;
        public event OnProjectionChanged ProjectionChanged;

        public FallbackCamera()
        {
            View = Matrix.CreateLookAt(Vector3.Up * CanyonGame.FarPlane/2, Vector3.Zero, Vector3.Forward);
            float a = (CanyonGame.FarPlane/4.0f) * CanyonGame.AspectRatio;
            float b = (CanyonGame.FarPlane/4.0f);
            Projection = Matrix.CreateOrthographicOffCenter(-a, a, -b, b, CanyonGame.NearPlane, CanyonGame.FarPlane);
        }
    }
}
