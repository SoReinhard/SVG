using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;

namespace Svg
{
    public class GraphicsPath
    {
        public List<GraphicsPath> Paths = new List<GraphicsPath>();
        public List<GraphicsFigure> Figures = new List<GraphicsFigure>();
        public List<GraphicsElement> Elements = new List<GraphicsElement>();

        public bool IsFigureOpen => _currentFigure != null;
        public bool IsEmpty => Paths.Count == 0 && Figures.Count == 0 && Elements.Count == 0;
        public Matrix3x2 CurrentTransform => _matrix;

        private GraphicsFigure _currentFigure;
        private Matrix3x2 _matrix;

        public GraphicsPath()
        {
            _matrix = Matrix3x2.CreateScale(1, 1);
        }

        public void AddPath(GraphicsPath path)
        {
            Paths.Add(path);
        }

        public void StartFigure()
        {
            _currentFigure = new GraphicsFigure();
            Figures.Add(_currentFigure);
        }

        public void AddElement(GraphicsElement element)
        {
            if (_currentFigure == null)
                Elements.Add(element);
            else
                _currentFigure.Elements.Add(element);
        }

        public void CloseFigure(bool closeGeometry = false)
        {
            if (_currentFigure != null)
            {
                _currentFigure.IsGeometryClosed = closeGeometry;
                _currentFigure = null;
            }
        }

        public void Transform(Matrix3x2 matrix)
        {
            _matrix = _matrix * matrix;
        }

        public IEnumerable<GraphicsElement> GetElements()
        {
            return Paths.SelectMany(p => p.GetElements()).Concat(Figures.SelectMany(f => f.GetElements())).Concat(Elements);
        }

        public GraphicsPath Clone()
        {
            return new GraphicsPath()
            {
                Paths = Paths.Select(p => p.Clone()).ToList(),
                Figures = Figures.Select(f => f.Clone()).ToList(),
                Elements = Elements.Select(e => e.Clone()).ToList(),

                _matrix = _matrix
            };
        }
    }
    public class GraphicsFigure
    {
        public bool IsGeometryClosed = false;
        public List<GraphicsElement> Elements = new List<GraphicsElement>();

        public List<GraphicsElement> GetElements()
        {
            if (!IsGeometryClosed || Elements.Count < 1 || !(Elements.First() is LineElement startLine) || !(Elements.Last() is LineElement endLine))
                return Elements;

            var closedElements = new List<GraphicsElement>(Elements);
            closedElements.Add(new LineElement(endLine.End, startLine.Start));
            return closedElements;
        }

        public GraphicsFigure Clone()
        {
            return new GraphicsFigure()
            {
                IsGeometryClosed = IsGeometryClosed,
                Elements = Elements.Select(e => e.Clone()).ToList()
            };
        }
    }
    public abstract class GraphicsElement
    {
        public abstract GraphicsElement Clone();
    }
    public class EllipseElement : GraphicsElement
    {
        public Vector2 Center;
        public float RadiusX;
        public float RadiusY;

        public EllipseElement(Vector2 center, float radiusX, float radiusY)
        {
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public override GraphicsElement Clone() => new EllipseElement(Center, RadiusX, RadiusY);
    }
    public class LineElement : GraphicsElement
    {
        public Vector2 Start;
        public Vector2 End;

        public LineElement(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public override GraphicsElement Clone() => new LineElement(Start, End);
    }
    public class RectangleElement : GraphicsElement
    {
        public Vector2 Location;
        public Vector2 Size;

        public Vector2 TopLeft => new Vector2(Location.X, Location.Y + Size.Y);
        public Vector2 TopRight => new Vector2(Location.X + Size.X, Location.Y + Size.Y);
        public Vector2 BottomLeft => new Vector2(Location.X, Location.Y);
        public Vector2 BottomRight => new Vector2(Location.X + Size.X, Location.Y);

        public RectangleElement(Vector2 location, Vector2 size)
        {
            Location = location;
            Size = size;
        }

        public override GraphicsElement Clone() => new RectangleElement(Location, Size);
    }
    public class BezierElement : LineElement
    {
        public Vector2 ControlPoint1;
        public Vector2 ControlPoint2;

        public BezierElement(Vector2 start, Vector2 cp1, Vector2 cp2, Vector2 end)
            : base(start, end)
        {
            ControlPoint1 = cp1;
            ControlPoint2 = cp2;
        }

        public BezierElement(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
            : base(new Vector2(x1, y1), new Vector2(x4, y4))
        {
            ControlPoint1 = new Vector2(x2, y2);
            ControlPoint2 = new Vector2(x3, y3);
        }

        public override GraphicsElement Clone() => new BezierElement(Start, ControlPoint1, ControlPoint2, End);
    }
    public class TextElement : GraphicsElement
    {
        public Vector2 Location;
        public float Size;
        public string Text;

        public TextElement(Vector2 location,float size,string text)
        {
            Location = location;
            Size = size;
            Text = text;
        }

        public override GraphicsElement Clone() => new TextElement(Location, Size, Text);
    }
}
