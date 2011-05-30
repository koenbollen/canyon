using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Canyon.Particles
{
    public class ParticleEmitter : GameComponent
    {
        protected ParticleSystem System;
        public ParticleEmitter(Game game, ParticleSystem system)
            :base(game)
        {
            this.System = system;
        }

        public override void Update(GameTime gameTime)
        {
            //this.system.AddParticle(Vector4.Zero);
        }
    }
}
