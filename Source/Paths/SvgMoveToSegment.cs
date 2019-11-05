using System.Numerics;

namespace Svg.Pathing
{
    public class SvgMoveToSegment : SvgPathSegment
    {
        public SvgMoveToSegment(Vector2 moveTo)
        {
            Start = moveTo;
            End = moveTo;
        }

        public override void AddToPath(GraphicsPath graphicsPath)
        {
            graphicsPath.StartFigure();
        }

        public override string ToString()
        {
            return "M" + Start.ToSvgString();
        }
    }
}
