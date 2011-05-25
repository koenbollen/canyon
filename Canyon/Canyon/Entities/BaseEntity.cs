using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Canyon.CameraSystem;
using Microsoft.Xna.Framework.Graphics;

namespace Canyon.Entities
{
    public class BaseEntity : DrawableGameComponent, IEntity, IFollowable
    {
        protected GameScreen Screen;

        public Vector3 Position { get; protected set; }
        public Quaternion Orientation { get; protected set; }

        public Vector3 Up { get { return Vector3.Transform(Vector3.Up, this.Orientation); } }
        public Vector3 Right { get { return Vector3.Transform(Vector3.Right, this.Orientation); } }
        public Vector3 Left { get { return Vector3.Transform(Vector3.Left, this.Orientation); } }
        public Vector3 Forward { get { return Vector3.Transform(Vector3.Forward, this.Orientation); } }

        // Semi physics:
        public Vector3 Velocity { get; protected set; }
        public Vector3 Acceleration { get; protected set; }
        public Vector3 Force { get; protected set; }
        public float Mass { get; protected set; }
        public float Drag { get; protected set; }

        public bool AffectedByGravity { get; protected set; }

        protected string asset = null;
        protected Model model = null;
        
        public BaseEntity(GameScreen screen, Vector3? position = null)
            : base(screen.Game)
        {
            this.Screen = screen;
            if (position.HasValue)
                this.Position = position.Value;
            this.Mass = 1.0f;
            this.Drag = 2f;
            this.AffectedByGravity = true;
            this.Orientation = Quaternion.Identity;
        }

        protected override void LoadContent()
        {
            if (asset != null)
                this.model = Game.Content.Load<Model>("Models/"+asset);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if( AffectedByGravity )
                this.Force += CanyonGame.Gravity;

            this.UpdatePhysics(dt);
            this.ClearForces();

            base.Update(gameTime);
        }

        private void UpdatePhysics(float dt)
        {
            this.Acceleration += Force / Mass;
            this.Acceleration -= Velocity * Drag;

            this.Velocity += this.Acceleration * dt;
            this.Position += this.Velocity * dt;
        }

        protected void ClearForces()
        {
            this.Force = Vector3.Zero;
            this.Acceleration = Vector3.Zero;
        }

        public void AddForce(Vector3 force)
        {
            this.Force += force;
        }



        public override void Draw(GameTime gameTime)
        {
            if (model == null)
                return;
            Matrix world = Matrix.CreateFromQuaternion(this.Orientation) * Matrix.CreateTranslation(this.Position);

            Matrix[] transforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = CanyonGame.Camera.View;
                    effect.Projection = CanyonGame.Camera.Projection;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    this.ApplyEffect(effect);
                }

                mesh.Draw();
            }
            base.Draw(gameTime);
        }

        protected virtual void ApplyEffect(BasicEffect effect)
        {
        }
    }
}
