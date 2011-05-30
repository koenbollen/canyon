using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Canyon.Particles
{
    public class ParticleSystem : DrawableGameComponent
    {
        protected static readonly Random Randy = new Random();

        public Matrix LocalWorld { get; protected set; }
        public int ParticleCount { get; protected set; }
        public string ParticleAsset { get; protected set; }
        public BlendState BlendState { get; protected set; }

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

        private bool ResetPhysics;

        public ParticleSystem(Game game)
            : base(game)
        {
            DrawOrder = 100;
        }

        public override void Initialize()
        {
            // Setup:
            LocalWorld = Matrix.Identity;
            ParticleCount = 100;
            ParticleAsset = "spot";
            BlendState = BlendState.AlphaBlend;
            LocalWorld = Matrix.CreateScale(10) *
                Matrix.CreateTranslation(new Vector3((CanyonGame.Screens.ActiveScreen as GameScreen).Terrain.Width/2, 0, (CanyonGame.Screens.ActiveScreen as GameScreen).Terrain.Height/2));

            ResetPhysics = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.effect = Game.Content.Load<Effect>("FX/particles");
            this.effect.CurrentTechnique = this.effect.Techniques["TransformAndTexture"];
            this.physics = Game.Content.Load<Effect>("FX/physics");
            this.batch = new SpriteBatch(GraphicsDevice);

            this.particle = Game.Content.Load<Texture2D>("Particles/" + this.ParticleAsset);

            SetupVertexData();
            CreateNoise();
            SetupRenderTargets();

            base.LoadContent();
        }

        private void SetupRenderTargets()
        {
            this.temporary = new RenderTarget2D(GraphicsDevice, ParticleCount, ParticleCount, true, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            this.positions = new RenderTarget2D(GraphicsDevice, ParticleCount, ParticleCount, true, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            this.velocities = new RenderTarget2D(GraphicsDevice, ParticleCount, ParticleCount, true, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
        }

        private void SetupVertexData()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[ParticleCount * ParticleCount * 4];
            for (int y = 0; y < ParticleCount; y++)
            {
                for (int x = 0; x < ParticleCount; x++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        VertexPositionTexture vert = new VertexPositionTexture();
                        vert.Position = new Vector3();
                        vert.Position.X = (float)y / (float)ParticleCount;
                        vert.Position.Y = (float)x / (float)ParticleCount;
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
                        vertices[((y * ParticleCount * 4) + (x * 4)) + i] = vert;
                    }
			    }
            }
            buffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            buffer.SetData<VertexPositionTexture>(vertices);

            int[] indices = new int[ParticleCount * ParticleCount * 6];
            int c = 0;
            for (int y = 0; y < ParticleCount; y++)
            {
                for (int x = 0; x < ParticleCount; x++)
                {
                    indices[c++] = ((x * ParticleCount * 4) + (y * 4)) + 0;
                    indices[c++] = ((x * ParticleCount * 4) + (y * 4)) + 1;
                    indices[c++] = ((x * ParticleCount * 4) + (y * 4)) + 2;
                    indices[c++] = ((x * ParticleCount * 4) + (y * 4)) + 0;
                    indices[c++] = ((x * ParticleCount * 4) + (y * 4)) + 2;
                    indices[c++] = ((x * ParticleCount * 4) + (y * 4)) + 3;
                }
            }

            index = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            index.SetData<Int32>(indices);
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

        private void PhysicsPass(string technique, RenderTarget2D result)
        {
            RenderTargetBinding[] old = GraphicsDevice.GetRenderTargets();
            GraphicsDevice.SetRenderTarget(temporary);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1, 0);

            physics.CurrentTechnique = physics.Techniques[technique];
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.DepthRead, RasterizerState.CullNone, physics);
            if (!ResetPhysics)
            {
                physics.Parameters["PositionMap"].SetValue(positions);
                physics.Parameters["VelocityMap"].SetValue(velocities);
            }
            batch.Draw(noise, new Rectangle(0, 0, ParticleCount, ParticleCount), Color.White);
            batch.End();

            GraphicsDevice.SetRenderTarget(result);
            physics.CurrentTechnique = physics.Techniques["CopyTexture"];
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, physics);
            batch.Draw(temporary, new Rectangle(0, 0, ParticleCount, ParticleCount), Color.White);
            batch.End();

            GraphicsDevice.SetRenderTargets(old);
        }

        public override void Update(GameTime gameTime)
        {
            physics.Parameters["ElapsedTime"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (ResetPhysics)
            {
                PhysicsPass("ResetVelocities", velocities);
                PhysicsPass("ResetPositions", positions);
                ResetPhysics = false;
            }
            else
            {
                PhysicsPass("UpdateVelocities", velocities);
                PhysicsPass("UpdatePositions", positions);
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            effect.Parameters["World"].SetValue(LocalWorld);
            effect.Parameters["CameraPosition"].SetValue(Matrix.Invert(CanyonGame.Camera.View).Translation);
            effect.Parameters["View"].SetValue(CanyonGame.Camera.View);
            effect.Parameters["Projection"].SetValue(CanyonGame.Camera.Projection);
            effect.Parameters["ParticleTexture"].SetValue(this.particle);
            effect.Parameters["PositionMap"].SetValue(this.positions);

            GraphicsDevice.BlendState = this.BlendState;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rs;

            GraphicsDevice.SetVertexBuffer(this.buffer);
            GraphicsDevice.Indices = this.index;
            effect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ParticleCount * ParticleCount * 4, 0, ParticleCount * ParticleCount * 2);
            
            base.Draw(gameTime);
        }

    }
}
