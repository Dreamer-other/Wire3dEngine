using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Wire3dEngine
{
    public static class QuaternionUtils
    {
        public static Quaternion Create(Vector3D axis, double angle)
        {
            var qx = axis.X * Math.Sin(angle / 2);
            var qy = axis.Y * Math.Sin(angle / 2);
            var qz = axis.Z * Math.Sin(angle / 2);
            var qw = Math.Cos(angle / 2);

            return new Quaternion(qx, qy, qz, qw);
        }
    }
}