using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Particles.Systems
{
    public class PlumeSystem : ParticleSystem
    {
        private Vector4? add;

        private int index;

        public PlumeSystem(Game game)
            :base(game)
        {
            DrawOrder = 100;
            index = 0;
            add = null;
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.ParticleAsset = "smoke";
            settings.ParticleCount = 100;
            settings.PhysicsName = "plume";
            settings.SizeModifier = 1f;
            settings.MinColor.A = 100;
            settings.MaxColor.A = 200;
            settings.MaxLife = 2.5f;
            settings.FadeAlpha = 1.0f;
        }

        public override void AddParticle(Vector4 data)
        {
            add = data;
        }

        protected override void ApplyPhysicsParamaters(Microsoft.Xna.Framework.Graphics.EffectParameterCollection parameters)
        {
            if (add.HasValue)
            {
                Vector2 uv = new Vector2(index % ParticleSize, index / ParticleSize);
                uv /= ParticleSize;
                index++;
                index %= ParticleSize * ParticleSize;
                parameters["NewParticle"].SetValue(uv);
                parameters["NewPosition"].SetValue(add.Value);
            }
            else
            {
                parameters["NewParticle"].SetValue(Vector2.Zero);
                parameters["NewPosition"].SetValue(Vector4.Zero);
            }
            add = null;
        }
    }
}
