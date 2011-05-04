using Microsoft.Xna.Framework;

namespace Canyon.Misc
{
    public struct YawPitchRoll
    {
        private Quaternion orientation;
        public Quaternion Orientation { get { return orientation; } }

        private float yaw;
        public float Yaw
        {
            get
            {
                return yaw;
            }
            set
            {
                if (yaw != value)
                {
                    yaw = MathHelper.WrapAngle(value);
                    UpdateOrientation();
                }
            }
        }

        private float pitch;
        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                if (pitch != value)
                {
                    pitch = MathHelper.WrapAngle(value);
                    UpdateOrientation();
                }
            }
        }

        private float roll;
        public float Roll
        {
            get
            {
                return roll;
            }
            set
            {
                if (roll != value)
                {
                    roll = MathHelper.WrapAngle(value);
                    UpdateOrientation();
                }
            }
        }

        private void UpdateOrientation()
        {
            this.orientation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        public override string ToString()
        {
            return string.Format("<YawPitchRoll {0:N} {1:N} {2:N}", this.yaw, this.pitch, this.roll);
        }
    }
}
