using System.Numerics;

namespace Svg.Pathing
{
    public sealed class SvgQuadraticCurveSegment : SvgPathSegment
    {
        public Vector2 ControlPoint { get; set; }

        private Vector2 FirstControlPoint
        {
            get
            {
                var x1 = Start.X + (ControlPoint.X - Start.X) * 2 / 3;
                var y1 = Start.Y + (ControlPoint.Y - Start.Y) * 2 / 3;

                return new Vector2(x1, y1);
            }
        }

        private Vector2 SecondControlPoint
        {
            get
            {
                var x2 = ControlPoint.X + (End.X - ControlPoint.X) / 3;
                var y2 = ControlPoint.Y + (End.Y - ControlPoint.Y) / 3;

                return new Vector2(x2, y2);
            }
        }

        public SvgQuadraticCurveSegment(Vector2 start, Vector2 controlPoint, Vector2 end)
        {
            Start = start;
            ControlPoint = controlPoint;
            End = end;
        }

        public override void AddToPath(GraphicsPath graphicsPath)
        {
            graphicsPath.AddElement(new BezierElement(Start, FirstControlPoint, SecondControlPoint, End));
        }

        public override string ToString()
        {
            return "Q" + ControlPoint.ToSvgString() + " " + End.ToSvgString();
        }
    }
}
