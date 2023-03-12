using System;
using System.Numerics;

namespace MetaFaceTrackingALXRtoVMCP.Movement
{
    internal class Gaze
    {
        public bool IsValid = false;
        public Quaternion Orientation = new();
        public Vector3 Position = new();

        private readonly float[] rawdataf = new float[8];

        public Gaze()
        {
        }

        public void Update(byte[] rawData, int index)
        {
            Buffer.BlockCopy(rawData, index, rawdataf, 0, rawdataf.Length * 4);
            IsValid = rawdataf[0] != 0;
            Orientation.X = rawdataf[1];
            Orientation.Y = rawdataf[2];
            Orientation.Z = rawdataf[3];
            Orientation.W = rawdataf[4];
            Position.X = rawdataf[5];
            Position.Y = rawdataf[6];
            Position.Z = rawdataf[7];
        }

        public double Yaw()
        {
            double yaw = Math.Atan2(
                2.0 * (Orientation.Y * Orientation.Z + Orientation.W * Orientation.X),
                Orientation.W * Orientation.W - Orientation.X * Orientation.X - Orientation.Y * Orientation.Y + Orientation.Z * Orientation.Z);
            return (180.0 / Math.PI) * yaw;
        }

        public double Pitch()
        {
            double pitch = Math.Asin(-2.0 * (Orientation.X * Orientation.Z - Orientation.W * Orientation.Y));
            return (180.0 / Math.PI) * pitch;
        }
    }
}
