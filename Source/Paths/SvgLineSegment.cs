using System.Numerics;

namespace Svg.Pathing
{
    public sealed class SvgLineSegment : SvgPathSegment
    {
        public SvgLineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public override void AddToPath(GraphicsPath graphicsPath)
        {
            graphicsPath.AddElement(new LineElement(Start, End));
        }

        public override string ToString()
        {
            return "L" + End.ToSvgString();
        }
    }
}
