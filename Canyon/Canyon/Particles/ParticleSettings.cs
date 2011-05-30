using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Particles
{
    public class ParticleSettings
    {
        /// <summary>
        /// The texture name of the particle.
        /// </summary>
        public string ParticleAsset = "spot";

        /// <summary>
        /// The number of Particles. Will be sqrt and pow2.
        /// </summary>
        public int ParticleCount = 10000;

        /// <summary>
        /// The maximum lifetime of each particle.
        /// </summary>
        public float MaxLife = 5.0f;

        /// <summary>
        /// The size of each particle.
        /// </summary>
        public float SizeModifier = 1.0f;

        /// <summary>
        /// The name of the physics effect to use.
        /// </summary>
        public string PhysicsName = "cube";

        /// <summary>
        /// The local world of the particle system.
        /// </summary>
        public Matrix LocalWorld = Matrix.Identity;

        /// <summary>
        /// The minimal color of the particle. Color will be chosen randomly between
        /// MinColor and MaxColor.
        /// </summary>
        public Color MinColor = Color.White;

        /// <summary>
        /// The maximal color of the particle. Color will be chosen randomly between
        /// MinColor and MaxColor.
        /// </summary>
        public Color MaxColor = Color.White;

        /// <summary>
        /// Number of seconds to start fading at the end of a particle's life.
        /// 0 == disabled.
        /// </summary>
        public float FadeAlpha = 0.0f;

        /// <summary>
        /// The blendstate to draw with, ex. AlphaBlend.
        /// </summary>
        public BlendState BlendState = BlendState.AlphaBlend;
    }
}
