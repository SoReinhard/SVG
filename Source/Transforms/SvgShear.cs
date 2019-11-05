using System.Globalization;
using System.Numerics;

namespace Svg.Transforms
{
    /// <summary>
    /// The class which applies the specified shear vector to this Matrix.
    /// </summary>
    public sealed class SvgShear : SvgTransform
    {
        public float X { get; set; }

        public float Y { get; set; }

        public override Matrix3x2 Matrix
        {
            get
            {
                return Matrix3x2.CreateSkew((float)System.Math.Atan(X), (float)System.Math.Atan(Y));
            }
        }

        public override string WriteToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "shear({0}, {1})", X, Y);
        }

        public SvgShear(float x)
            : this(x, x)
        {
        }

        public SvgShear(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override object Clone()
        {
            return new SvgShear(X, Y);
        }
    }
}
