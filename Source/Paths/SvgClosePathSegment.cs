namespace Svg.Pathing
{
    public sealed class SvgClosePathSegment : SvgPathSegment
    {
        public override void AddToPath(GraphicsPath graphicsPath)
        {
            graphicsPath.CloseFigure(true);
        }

        public override string ToString()
        {
            return "z";
        }
    }
}
