using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Canyon.Misc
{
    public struct LerpVector3
    {
        private Vector3 start;
        private Vector3 goal;
        private Vector3 real;
        public Vector3 Value
        {
            get
            {
                return real;
            }
            set
            {
                if (real != value)
                {
                    if( (real - value).Length() != 0 )
                     CanyonGame.Console.Debug("(real - value).Length() " + (real - value).Length());
                    if (time > 0 || (real - value).Length() > threshold)
                    {
                        goal = value;
                        if (time <= 0)
                        {
                            time = delay;
                            start = real;
                        }
                    }
                    else
                    {
                        goal = real = value;
                    }

                }
            }

        }

        private float delay;
        private float threshold;
        private float threshold2;
        private float time;

        public LerpVector3( float delay, float threshold )
        {
            time = 0;
            start = goal = real = Vector3.Zero;
            this.delay = delay;
            this.threshold = threshold;
            threshold2 = threshold * threshold;
        }

        public void Reset()
        {
            time = 0;
            real = goal;
        }

        public void Step(float dt)
        {
            if (time > 0)
            {
                real = Vector3.Lerp(goal, start, time / delay);
                time -= dt;
            }
            else
            {
                time = 0;
                real = goal;
            }
        }

        public override string ToString()
        {
            return real + " " + goal + " " + start + " " + time;
        }


    }
}
