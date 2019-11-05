using System.Globalization;
using System.Numerics;

namespace Svg.Transforms
{
    public sealed class SvgScale : SvgTransform
    {
        public float X { get; set; }

        public float Y { get; set; }

        public override Matrix3x2 Matrix
        {
            get
            {
                return Matrix3x2.CreateScale(X, Y);
            }
        }

        public override string WriteToString()
        {
            if (X == Y)
                return string.Format(CultureInfo.InvariantCulture, "scale({0})", X);
            return string.Format(CultureInfo.InvariantCulture, "scale({0}, {1})", X, Y);
        }

        public SvgScale(float x)
            : this(x, x)
        {
        }

        public SvgScale(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override object Clone()
        {
            return new SvgScale(X, Y);
        }
    }
}
