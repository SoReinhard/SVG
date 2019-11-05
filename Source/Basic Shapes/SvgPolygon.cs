using System.Diagnostics;
using System.Numerics;

namespace Svg
{
    /// <summary>
    /// SvgPolygon defines a closed shape consisting of a set of connected straight line segments.
    /// </summary>
    [SvgElement("polygon")]
    public class SvgPolygon : SvgMarkerElement
    {
        private GraphicsPath _path;

        /// <summary>
        /// The points that make up the SvgPolygon
        /// </summary>
        [SvgAttribute("points")]
        public SvgPointCollection Points
        {
            get { return GetAttribute<SvgPointCollection>("points", false); }
            set { Attributes["points"] = value; IsPathDirty = true; }
        }

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (this._path == null || this.IsPathDirty)
            {
                this._path = new GraphicsPath();
                this._path.StartFigure();

                try
                {
                    var points = this.Points;
                    Vector2 lastPoint = Vector2.Zero;
                    for (int i = 2; (i + 1) < points.Count; i += 2)
                    {
                        var endPoint = SvgUnit.GetDevicePoint(points[i], points[i + 1], renderer, this);

                        // If it is to render, don't need to consider stroke width.
                        // i.e stroke width only to be considered when calculating boundary
                        if (renderer == null)
                        {
                            var radius = base.StrokeWidth * 2;
                            _path.AddElement(new EllipseElement(endPoint, radius, radius));
                            continue;
                        }

                        //first line
                        if (i == 2)
                        {
                            _path.AddElement(new LineElement(SvgUnit.GetDevicePoint(points[i - 2], points[i - 1], renderer, this), endPoint));
                        }
                        else
                        {
                            _path.AddElement(new LineElement(lastPoint, endPoint));
                        }

                        lastPoint = endPoint;
                    }
                }
                catch
                {
                    Trace.TraceError("Error parsing points");
                }

                this._path.CloseFigure(true);
                if (renderer != null)
                    this.IsPathDirty = false;
            }
            return this._path;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgPolygon>();
        }
    }
}
