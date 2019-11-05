using System;
using System.Globalization;
using System.Numerics;

namespace Svg.Transforms
{
    /// <summary>
    /// The class which applies the specified skew vector to this Matrix.
    /// </summary>
    public sealed class SvgSkew : SvgTransform
    {
        public float AngleX { get; set; }

        public float AngleY { get; set; }

        public override Matrix3x2 Matrix
        {
            get
            {
                return Matrix3x2.CreateSkew(MathHelper.DegToRad * AngleX, MathHelper.DegToRad * AngleY);
            }
        }

        public override string WriteToString()
        {
            if (AngleY == 0f)
                return string.Format(CultureInfo.InvariantCulture, "skewX({0})", AngleX);
            return string.Format(CultureInfo.InvariantCulture, "skewY({0})", AngleY);
        }

        public SvgSkew(float x, float y)
        {
            AngleX = x;
            AngleY = y;
        }

        public override object Clone()
        {
            return new SvgSkew(AngleX, AngleY);
        }
    }
}
