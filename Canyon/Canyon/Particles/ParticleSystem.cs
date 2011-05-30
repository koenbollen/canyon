using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Canyon.Particles
{
    public abstract class ParticleSystem : DrawableGameComponent
    {
        protected static readonly Random Randy = new Random();

        public ParticleSettings Settings { get; protected set; }

        protected int ParticleSize;

        private Effect effect;
        private Effect physics;
        private SpriteBatch batch;

        private Texture2D particle;
        private Texture2D noise;
        private RenderTarget2D temporary;
        private RenderTarget2D positions;
        private RenderTarget2D velocities;
        private VertexBuffer buffer;
        private IndexBuffer index;
        private bool resetPhysics;

        public ParticleSystem(Game game)
            : base(game)
        {
        }

        protected abstract void InitializeSettings(ParticleSettings settings);

        public override void Initialize()
        {
            this.Settings = new ParticleSettings();
            InitializeSettings(this.Settings);

            ParticleSize = (int)Math.Sqrt(this.Settings.ParticleCount);

            resetPhysics = true;
            base.Initialize();
        }

        public virtual void AddParticle(Vector4 data)
        {
        }

        protected override void LoadContent()
        {
            this.effect = Game.Content.Load<Effect>("FX/particles");
            this.effect.CurrentTechnique = this.effect.Techniques["TransformAndTexture"];
            this.physics = Game.Content.Load<Effect>("FX/physics/"+this.Settings.PhysicsName);
            this.batch = new SpriteBatch(GraphicsDevice);

            this.particle = Game.Content.Load<Texture2D>("Particles/" + this.Settings.ParticleAsset);

            CreateNoise();
            SetupVertexData();
            SetupRenderTargets();

            base.LoadContent();
        }

        private void CreateNoise()
        {
            this.noise = new Texture2D(GraphicsDevice, 128, 128, true, SurfaceFormat.Vector4);
            Vector4[] data = new Vector4[128 * 128];
            for (int i = 0; i < 128 * 128; i++)
            {
                data[i] = new Vector4();
                data[i].X = (float)Randy.NextDouble() - 0.5f;
                data[i].Y = (float)Randy.NextDouble() - 0.5f;
                data[i].Z = (float)Randy.NextDouble() - 0.5f;
                data[i].W = (float)Randy.NextDouble() - 0.5f;
            }
            this.noise.SetData<Vector4>(data);
        }

        private void SetupVertexData()
        {
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[ParticleSize * ParticleSize * 4];
            for (int y = 0; y < ParticleSize; y++)
            {
                for (int x = 0; x < ParticleSize; x++)
                {
                    Color col = Color.Lerp(this.Settings.MinColor, this.Settings.MaxColor, (float)Randy.NextDouble());
                    for (int i = 0; i < 4; i++)
                    {
                        VertexPositionColorTexture vert = new VertexPositionColorTexture();
                        vert.Color = col;
                        vert.Position = new Vector3();
                        vert.Position.X = (float)y / (float)ParticleSize;
                        vert.Position.Y = (float)x / (float)ParticleSize;
                        vert.Position.Z = (float)i;
                        switch( i )
                        {
                            case 0:
                                vert.TextureCoordinate = new Vector2(0, 0);
                                break;
                            case 1:
                                vert.TextureCoordinate = new Vector2(1, 0);
                                break;
                            case 2:
                                vert.TextureCoordinate = new Vector2(1, 1);
                                break;
                            case 3:
                                vert.TextureCoordinate = new Vector2(0, 1);
                                break;
                        }
                        vertices[((y * ParticleSize * 4) + (x * 4)) + i] = vert;
                    }
			    }
            }
            buffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            buffer.SetData<VertexPositionColorTexture>(vertices);

            int[] indices = new int[ParticleSize * ParticleSize * 6];
            int c = 0;
            for (int y = 0; y < ParticleSize; y++)
            {
                for (int x = 0; x < ParticleSize; x++)
                {
                    indices[c++] = ((x * ParticleSize * 4) + (y * 4)) + 0;
                    indices[c++] = ((x * ParticleSize * 4) + (y * 4)) + 1;
                    indices[c++] = ((x * ParticleSize * 4) + (y * 4)) + 2;
                    indices[c++] = ((x * ParticleSize * 4) + (y * 4)) + 0;
                    indices[c++] = ((x * ParticleSize * 4) + (y * 4)) + 2;
                    indices[c++] = ((x * ParticleSize * 4) + (y * 4)) + 3;
                }
            }

            index = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            index.SetData<Int32>(indices);
        }

        private void SetupRenderTargets()
        {
            this.temporary = new RenderTarget2D(GraphicsDevice, ParticleSize, ParticleSize, true, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            this.positions = new RenderTarget2D(GraphicsDevice, ParticleSize, ParticleSize, true, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            this.velocities = new RenderTarget2D(GraphicsDevice, ParticleSize, ParticleSize, true, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
        }

        private void PhysicsPass(string technique, RenderTarget2D result)
        {
            RenderTargetBinding[] old = GraphicsDevice.GetRenderTargets();
            GraphicsDevice.SetRenderTarget(temporary);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1, 0);

            physics.CurrentTechnique = physics.Techniques[technique];
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.DepthRead, RasterizerState.CullNone, physics);
            if (!resetPhysics)
            {
                physics.Parameters["PositionMap"].SetValue(positions);
                physics.Parameters["VelocityMap"].SetValue(velocities);
            }
            batch.Draw(noise, new Rectangle(0, 0, ParticleSize, ParticleSize), Color.White);
            batch.End();

            GraphicsDevice.SetRenderTarget(result);
            physics.CurrentTechnique = physics.Techniques["CopyTexture"];
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, physics);
            batch.Draw(temporary, new Rectangle(0, 0, ParticleSize, ParticleSize), Color.White);
            batch.End();

            GraphicsDevice.SetRenderTargets(old);
        }

        protected virtual void ApplyPhysicsParamaters(EffectParameterCollection parameters)
        {
        }

        public override void Update(GameTime gameTime)
        {
            this.physics.Parameters["ElapsedTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
            this.physics.Parameters["MaxLife"].SetValue(this.Settings.MaxLife);
            this.ApplyPhysicsParamaters(this.physics.Parameters);
            if (this.resetPhysics)
            {
                this.PhysicsPass("ResetVelocities", this.velocities);
                this.PhysicsPass("ResetPositions", this.positions);
                this.resetPhysics = false;
            }
            else
            {
                this.PhysicsPass("UpdateVelocities", this.velocities);
                this.PhysicsPass("UpdatePositions", this.positions);
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            effect.Parameters["World"].SetValue(this.Settings.LocalWorld);
            effect.Parameters["CameraPosition"].SetValue(Matrix.Invert(CanyonGame.Camera.View).Translation);
            effect.Parameters["View"].SetValue(CanyonGame.Camera.View);
            effect.Parameters["Projection"].SetValue(CanyonGame.Camera.Projection);
            effect.Parameters["ParticleTexture"].SetValue(this.particle);
            effect.Parameters["PositionMap"].SetValue(this.positions);
            effect.Parameters["SizeModifier"].SetValue(this.Settings.SizeModifier);
            effect.Parameters["MaxLife"].SetValue(this.Settings.MaxLife);
            effect.Parameters["FadeAlpha"].SetValue(this.Settings.FadeAlpha);

            GraphicsDevice.BlendState = this.Settings.BlendState;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.SetVertexBuffer(this.buffer);
            GraphicsDevice.Indices = this.index;
            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, buffer.VertexCount, 0, index.IndexCount/3);
            
            base.Draw(gameTime);
        }

    }
}
