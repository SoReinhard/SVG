using System.Numerics;

namespace Svg.Pathing
{
    public abstract class SvgPathSegment
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }

        protected SvgPathSegment()
        {
        }

        protected SvgPathSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public abstract void AddToPath(GraphicsPath graphicsPath);

        public SvgPathSegment Clone()
        {
            return MemberwiseClone() as SvgPathSegment;
        }
    }
}
