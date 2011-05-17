using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Entities
{
    public class Marker : DrawableGameComponent
    {
        public const float RotationSpeed = MathHelper.Pi / 8;
        public const float LevitateSpeed = .4f;
        public const float LevitateRate = .6f;

        private Model model;

        private Vector3 position;
        private float alpha;

        private int direction;
        private float rotation;
        private float height;
        private double heightstep;

        protected GameScreen Screen;
        public Marker(GameScreen screen, Vector3 position)
            :base(screen.Game)
        {
            this.Screen = screen;
            this.position = position;
            DrawOrder = 5;
        }

        public override void Initialize()
        {
            this.alpha = 0.8f;
            this.rotation = 0;
            this.direction = CanyonGame.Randy.NextDouble() < .5f ? -1 : 1;
            this.heightstep = CanyonGame.Randy.NextDouble();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            model = Game.Content.Load<Model>("Models/marker");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this.rotation = MathHelper.WrapAngle(this.rotation + (direction * RotationSpeed * dt));

            this.height = (float)Math.Sin(heightstep) * LevitateRate;
            heightstep += dt * LevitateSpeed;
            heightstep %= Math.PI;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix world = Matrix.CreateRotationY(this.rotation) * Matrix.CreateTranslation(this.position + (Vector3.Up * height));

            Matrix[] transforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = CanyonGame.Camera.View;
                    effect.Projection = CanyonGame.Camera.Projection;
                    effect.Alpha = this.alpha;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
