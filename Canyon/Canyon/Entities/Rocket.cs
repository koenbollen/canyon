using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Entities
{
    public class Rocket : BaseEntity
    {
        public class RocketDeploy : GameComponent
        {
            private const float DeployTime = 2.3f;
            private const float RechargeTime = 1.0f;
            private enum DeployState
            {
                Waiting,
                Deploying,
                Recharging,
            };
            private Player player;
            protected GameScreen Screen;
            private DeployState state;
            private float time;
            private Rocket current;
            public RocketDeploy(GameScreen screen, Player player)
                :base( screen.Game )
            {
                this.Screen = screen;
                this.player = player;
                Screen.Game.Content.Load<Model>("Models/rocket");//preload;
                this.state = DeployState.Waiting;
            }

            public void Deploy()
            {
                if (this.state == DeployState.Waiting)
                {
                    this.state = DeployState.Deploying;
                    this.time = DeployTime;
                    this.current = new Rocket(Screen, player.Position);
                    this.UpdateWorld();
                    current.Initialize();
                    Screen.Components.Add(current);
                }
            }

            private void UpdateWorld()
            {
                this.current.Position = player.Position + Vector3.Transform(Vector3.Down, this.player.Orientation) * .1f;
                this.current.Velocity = this.player.Velocity;
                this.current.Orientation = player.Orientation;
            }

            public override void Update(GameTime gameTime)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (this.time > 0)
                    this.time -= dt;

                if (this.state == DeployState.Deploying)
                {
                    this.UpdateWorld();
                    if (this.time <= 0)
                    {
                        time = RechargeTime;
                        this.state = DeployState.Recharging;
                        this.current.Fire();
                        this.current = null;
                    }
                }
                if (this.state == DeployState.Recharging && this.time <= 0)
                    this.state = DeployState.Waiting;


                base.Update(gameTime);
            }

        }

        public const float BurnTime = 4.0f;
        public const float Thrust = 100.0f;

        private float burntime;

        public Rocket(GameScreen screen, Vector3? position)
            : base(screen, position)
        {
            this.burntime = 0;
            this.asset = "rocket";
            this.AffectedByGravity = false;
            this.Mass = 2f;
            this.Drag = 0.5f;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if( this.burntime > 0 )
            {
                this.AddForce(this.Forward * Thrust);
                burntime -= dt;
            }

            Vector3 viewPosition = Matrix.Invert(CanyonGame.Camera.View).Translation;
            if ((this.Position - viewPosition).LengthSquared() > CanyonGame.FarPlane * CanyonGame.FarPlane)
            {
                this.Screen.Components.Remove(this);
                return;
            }

            base.Update(gameTime);
        }

        public void Fire()
        {
            if( this.burntime == 0 )
                this.burntime = BurnTime;
        }

    }
}
