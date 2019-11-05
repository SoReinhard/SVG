using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Svg
{
    public static class ReplacementExtensions
    {
        public static Vector2 ToNumeric(this PointF point) => new Vector2(point.X, point.Y);
        public static PointF ToDrawing(this Vector2 vector) => new PointF(vector.X, vector.Y);
    }
}
