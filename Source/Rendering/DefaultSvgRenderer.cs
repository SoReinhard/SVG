using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Linq;

namespace Svg
{
    public class DefaultSvgRenderer : ISvgRenderer
    {
        private Matrix3x2 _transform;
        public Matrix3x2 Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        public List<SvgRendererPath> Paths = new List<SvgRendererPath>();

        public void DrawPath(GraphicsPath path, Color color, float width)
        {
            Paths.Add(new SvgRendererPath()
            {
                Transform = Transform,
                Path = path,
                Pen = new SvgRendererStrokePen() { Color = color, Width = width }
            });
        }
        public void FillPath(GraphicsPath path, Color color)
        {
            Paths.Add(new SvgRendererPath()
            {
                Transform = Transform,
                Path = path,
                Pen = new SvgRendererFillPen() { Color = color }
            });
        }

        public DefaultSvgRenderer()
        {
            Transform = Matrix3x2.CreateScale(1, 1);
        }

        public void RotateTransform(float fAngle)
        {
            Transform = Matrix3x2.CreateRotation(MathHelper.DegToRad * fAngle) * Transform;
        }

        public void ScaleTransform(float sx, float sy)
        {
            Transform = Matrix3x2.CreateScale(sx, sy) * Transform;
        }

        public void TranslateTransform(float dx, float dy)
        {
            Transform = Matrix3x2.CreateTranslation(dx, dy) * Transform;
        }

        public List<Tuple<Matrix3x2, List<GraphicsElement>>> GetElements(Matrix3x2 globalTransform)
        {
            return Paths.Select(p => p.GetElements(globalTransform)).ToList();
        }
    }
    public class SvgRendererPath
    {
        public Matrix3x2 Transform;
        public GraphicsPath Path;
        public SvgRendererPen Pen;

        public Tuple<Matrix3x2, List<GraphicsElement>> GetElements(Matrix3x2 globalTransform)
        {
            return Tuple.Create(Transform * Path.CurrentTransform * globalTransform, Path.GetElements().ToList());
        }
    }
    public abstract class SvgRendererPen
    {
        public Color Color;
    }
    public class SvgRendererStrokePen : SvgRendererPen
    {
        public float Width;
    }
    public class SvgRendererFillPen : SvgRendererPen
    {

    }
}
