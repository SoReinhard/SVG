using System.Numerics;

namespace Svg.Pathing
{
    public sealed class SvgCubicCurveSegment : SvgPathSegment
    {
        public Vector2 FirstControlPoint { get; set; }
        public Vector2 SecondControlPoint { get; set; }

        public SvgCubicCurveSegment(Vector2 start, Vector2 firstControlPoint, Vector2 secondControlPoint, Vector2 end)
        {
            Start = start;
            End = end;
            FirstControlPoint = firstControlPoint;
            SecondControlPoint = secondControlPoint;
        }

        public override void AddToPath(GraphicsPath graphicsPath)
        {
            graphicsPath.AddElement(new BezierElement(Start, FirstControlPoint, SecondControlPoint, End));
        }

        public override string ToString()
        {
            return "C" + FirstControlPoint.ToSvgString() + " " + SecondControlPoint.ToSvgString() + " " + End.ToSvgString();
        }
    }
}
