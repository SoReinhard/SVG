using System.Numerics;

namespace Svg
{
    /// <summary>
    /// Represents and SVG line element.
    /// </summary>
    [SvgElement("line")]
    public class SvgLine : SvgMarkerElement
    {
        private SvgUnit _startX = 0f;
        private SvgUnit _startY = 0f;
        private SvgUnit _endX = 0f;
        private SvgUnit _endY = 0f;

        private GraphicsPath _path;

        [SvgAttribute("x1")]
        public SvgUnit StartX
        {
            get { return _startX; }
            set
            {
                if (_startX != value)
                {
                    _startX = value;
                    IsPathDirty = true;
                }
                Attributes["x1"] = value;
            }
        }

        [SvgAttribute("y1")]
        public SvgUnit StartY
        {
            get { return _startY; }
            set
            {
                if (_startY != value)
                {
                    _startY = value;
                    IsPathDirty = true;
                }
                Attributes["y1"] = value;
            }
        }

        [SvgAttribute("x2")]
        public SvgUnit EndX
        {
            get { return _endX; }
            set
            {
                if (_endX != value)
                {
                    _endX = value;
                    IsPathDirty = true;
                }
                Attributes["x2"] = value;
            }
        }

        [SvgAttribute("y2")]
        public SvgUnit EndY
        {
            get { return _endY; }
            set
            {
                if (_endY != value)
                {
                    _endY = value;
                    IsPathDirty = true;
                }
                Attributes["y2"] = value;
            }
        }

        public override SvgPaintServer Fill
        {
            get { return null; /* Line can't have a fill */ }
            set
            {
                // Do nothing
            }
        }

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if ((this._path == null || this.IsPathDirty) && base.StrokeWidth > 0)
            {
                Vector2 start = new Vector2(this.StartX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                          this.StartY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));
                Vector2 end = new Vector2(this.EndX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                        this.EndY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));

                this._path = new GraphicsPath();

                // If it is to render, don't need to consider stroke width.
                // i.e stroke width only to be considered when calculating boundary
                if (renderer != null)
                {
                    this._path.AddElement(new LineElement(start, end));
                    this.IsPathDirty = false;
                }
                else
                {    // only when calculating boundary 
                    _path.StartFigure();
                    var radius = base.StrokeWidth / 2;
                    _path.AddElement(new EllipseElement(start, radius, radius));
                    _path.AddElement(new EllipseElement(end, radius, radius));
                    _path.CloseFigure();
                }
            }
            return this._path;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgLine>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgLine;

            newObj._startX = _startX;
            newObj._startY = _startY;
            newObj._endX = _endX;
            newObj._endY = _endY;
            return newObj;
        }
    }
}
