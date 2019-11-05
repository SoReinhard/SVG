using System.Globalization;
using System.Numerics;

namespace Svg.Transforms
{
    public sealed class SvgTranslate : SvgTransform
    {
        public float X { get; set; }

        public float Y { get; set; }

        public override Matrix3x2 Matrix
        {
            get
            {
                return Matrix3x2.CreateTranslation(X, Y);
            }
        }

        public override string WriteToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "translate({0}, {1})", X, Y);
        }

        public SvgTranslate(float x, float y)
        {
            X = x;
            Y = y;
        }

        public SvgTranslate(float x)
            : this(x, 0f)
        {
        }

        public override object Clone()
        {
            return new SvgTranslate(X, Y);
        }
    }
}
