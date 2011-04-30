using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Canyon.Misc
{
    public class DrawableVector
    {
        private Vector3 position;
        private Vector3 direction;
        private Color color;

        public Vector3 Position
        {
            get { return position; }
            set { position = value; VectorDrawer.TriggerUpdate(); }
        }
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; VectorDrawer.TriggerUpdate(); }
        }
        public Color Color
        {
            get { return color; }
            set { color = value; VectorDrawer.TriggerUpdate(); }
        }

        public DrawableVector(Vector3 position, Vector3 direction, Color color)
        {
            this.position = position;
            this.direction = direction;
            this.color = color;
        }

        public DrawableVector(Vector3 position, Vector3 direction)
            : this(position, direction, Color.Black)
        {
        }
    }

    public class VectorDrawer : DrawableGameComponent
    {
        public static VectorDrawer Instance { get; private set; }

        public static DrawableVector AddVector(DrawableVector vector)
        {
            if (VectorDrawer.Instance != null)
                return VectorDrawer.Instance.addVector(vector);
            return vector;
        }
        public static DrawableVector AddVector(Vector3 position, Vector3 direction, Color color)
        {
            return VectorDrawer.AddVector(new DrawableVector(position, direction, color));
        }
        public static DrawableVector AddVector(Vector3 position, Vector3 direction)
        {
            return VectorDrawer.AddVector(position, direction, Color.Black);
        }

        public static ReadOnlyCollection<DrawableVector> Vectors
        {
            get { return VectorDrawer.Instance.vectors.AsReadOnly(); }
        }

        public static void TriggerUpdate()
        {
            if (Instance != null)
                Instance.triggerUpdate();
        }
        
        private List<DrawableVector> vectors;
        private bool listChanged;
        private List<VertexPositionColor> vertices;

        private BasicEffect effect;

        public VectorDrawer(Game game)
            : base(game)
        {
            VectorDrawer.Instance = this;

            vectors = new List<DrawableVector>();
            vertices = new List<VertexPositionColor>();
            listChanged = false;

#if DEBUG
            // Remember: XYZ is RGB!
            AddVector(Vector3.Zero, Vector3.UnitX, Color.Red);
            AddVector(Vector3.Zero, Vector3.UnitY, Color.Green);
            AddVector(Vector3.Zero, Vector3.UnitZ, Color.Blue);
#endif // DEBUG

            DrawOrder = 100;
        }

        protected override void LoadContent()
        {
            effect = new BasicEffect(GraphicsDevice);
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (listChanged)
            {
                vertices.Clear();
                for( int i = 0; i < vectors.Count; i++ )
                {
                    DrawableVector v = vectors[i];
                    vertices.Add(new VertexPositionColor(v.Position, v.Color));
                    vertices.Add(new VertexPositionColor(v.Position + v.Direction, v.Color));
                }
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (vertices.Count < 2)
                return;

            effect.View = CanyonGame.Camera.View;
            effect.Projection = CanyonGame.Camera.Projection;
            effect.VertexColorEnabled = true;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices.ToArray(), 0, vertices.Count / 2);
            }
            base.Draw(gameTime);
        }

        private void triggerUpdate()
        {
            listChanged = true;
        }

        private DrawableVector addVector(DrawableVector vector)
        {
            vectors.Add(vector);
            triggerUpdate();
            return vector;
        }
    }

    public static class VectorDrawerHelper
    {
        public static DrawableVector Draw(this Vector3 direction, Vector3 position, Color color)
        {
            return VectorDrawer.AddVector(position, direction, color);
        }
        public static DrawableVector Draw(this Vector3 direction, Vector3 position)
        {
            return VectorDrawer.AddVector(position, direction, Color.Black);
        }
    }
}
