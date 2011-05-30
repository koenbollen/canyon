using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Canyon.Particles.Emitters
{
    public class PositionEmitter : ParticleEmitter
    {
        float time = 0;
        private Vector3 position;
        public PositionEmitter(Game game, ParticleSystem system, Vector3 position)
            : base(game, system)
        {
            this.position = position;
            time = .2f;
        }

        public override void Update(GameTime gameTime)
        {
            if( time <= 0 )
            {
                this.System.AddParticle(new Vector4(this.position, this.System.Settings.MaxLife));
                time += .1f;
            }
            time -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
