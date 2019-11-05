using System;
using System.Drawing;
using System.Numerics;

namespace Svg
{
    public interface ISvgRenderer
    {
        Matrix3x2 Transform { get; set; }

        void DrawPath(GraphicsPath path, Color color, float width);
        void FillPath(GraphicsPath path, Color color);

        void TranslateTransform(float dx, float dy);
        void RotateTransform(float fAngle);
        void ScaleTransform(float sx, float sy);
    }
}
