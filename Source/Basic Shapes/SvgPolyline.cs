using System;
using System.Drawing;
using System.Diagnostics;
using System.Numerics;

namespace Svg
{
    /// <summary>
    /// SvgPolyline defines a set of connected straight line segments. Typically, <see cref="SvgPolyline"/> defines open shapes.
    /// </summary>
    [SvgElement("polyline")]
    public class SvgPolyline : SvgPolygon
    {
        private GraphicsPath _Path;

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if ((_Path == null || this.IsPathDirty) && base.StrokeWidth > 0)
            {
                _Path = new GraphicsPath();

                try
                {
                    Vector2 lastPoint = Vector2.Zero;
                    for (int i = 0; (i + 1) < Points.Count; i += 2)
                    {
                        Vector2 endPoint = new Vector2(Points[i].ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                                     Points[i + 1].ToDeviceValue(renderer, UnitRenderingType.Vertical, this));

                        if (renderer == null)
                        {
                            var radius = base.StrokeWidth / 2;
                            _Path.AddElement(new EllipseElement(endPoint, radius, radius));
                            continue;
                        }

                        // TODO: Remove unrequired first line
                        if (_Path.IsEmpty)
                        {
                            _Path.AddElement(new LineElement(endPoint, endPoint));
                        }
                        else
                        {
                            _Path.AddElement(new LineElement(lastPoint, endPoint));
                        }

                        lastPoint = endPoint;
                    }
                }
                catch (Exception exc)
                {
                    Trace.TraceError("Error rendering points: " + exc.Message);
                }
                if (renderer != null)
                    this.IsPathDirty = false;
            }
            return _Path;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgPolyline>();
        }
    }
}
