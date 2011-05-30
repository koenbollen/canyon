using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Particles.Systems
{
    public class SnowSystem : ParticleSystem
    {
        public SnowSystem(Game game)
            : base(game)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.LocalWorld = Matrix.CreateScale(10) *
                Matrix.CreateTranslation(new Vector3((CanyonGame.Screens.ActiveScreen as GameScreen).Terrain.Width / 2, 0, (CanyonGame.Screens.ActiveScreen as GameScreen).Terrain.Height / 2));

            settings.SizeModifier = .2f;
            settings.MinColor = Color.White;
            settings.MinColor.A = 150;
            settings.MaxColor = Color.LightGray;
            settings.MaxColor.A = 200;
            settings.MaxLife = 15.0f;
        }

        protected override void ApplyPhysicsParamaters(EffectParameterCollection parameters)
        {
            parameters["GravityY"].SetValue(-.0984f);
        }
    }
}
