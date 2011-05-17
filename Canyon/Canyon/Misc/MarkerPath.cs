using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Canyon.Entities;

namespace Canyon.Misc
{
    public class MarkerPath : GameComponent
    {
        private int count;
        private Vector2 stepBounds;
        private float angleBound;

        private Vector3 start;
        private List<Marker> markers;
        private Vector3 initialDirection;

        protected GameScreen Screen;
        public MarkerPath(GameScreen screen, Vector3 start, Vector3 initialDirection, int count)
            : base(screen.Game)
        {
            this.Screen = screen;

            this.markers = new List<Marker>();
            this.start = start;
            this.initialDirection = initialDirection;
            this.count = count;

            stepBounds = new Vector2(20, 54);
            angleBound = MathHelper.Pi / 8;
            createMarkers();
        }

        private void createMarkers()
        {
            Vector3 position = start;
            Vector3 direction = this.initialDirection;
            Screen.Components.Add(new Marker(Screen, position));
            for (int i = 1; i < count; i++)
            {
                Vector3 right = Vector3.Cross(Vector3.Up, direction);
                Vector3 up = Vector3.Cross(right, direction);
                right = Vector3.Cross(up, direction);
                Matrix rot = Matrix.CreateFromAxisAngle(up, (float)CanyonGame.Randy.NextDouble() * angleBound)
                    * Matrix.CreateFromAxisAngle(right, (float)CanyonGame.Randy.NextDouble() * angleBound);
                direction = Vector3.TransformNormal(direction, rot);
                float distance = ((float)CanyonGame.Randy.NextDouble() * stepBounds.Y - stepBounds.X) + stepBounds.X;
                position += direction * distance;
                Marker m = new Marker(Screen, position);
                markers.Add(m);
                Screen.Components.Add(m);
            }
       
        }
    }
}
