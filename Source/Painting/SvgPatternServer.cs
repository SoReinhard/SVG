using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Svg.Transforms;

namespace Svg
{
    /// <summary>
    /// A pattern is used to fill or stroke an object using a pre-defined graphic object which can be replicated ("tiled") at fixed intervals in x and y to cover the areas to be painted.
    /// </summary>
    [SvgElement("pattern")]
    public sealed class SvgPatternServer : SvgPaintServer, ISvgViewPort
    {
        private SvgUnit _x = SvgUnit.None;
        private SvgUnit _y = SvgUnit.None;
        private SvgUnit _width = SvgUnit.None;
        private SvgUnit _height = SvgUnit.None;
        private SvgCoordinateUnits _patternUnits = SvgCoordinateUnits.Inherit;
        private SvgCoordinateUnits _patternContentUnits = SvgCoordinateUnits.Inherit;
        private SvgViewBox _viewBox;

        /// <summary>
        /// Gets or sets the X-axis location of the pattern.
        /// </summary>
        [SvgAttribute("x")]
        public SvgUnit X
        {
            get { return _x; }
            set { _x = value; Attributes["x"] = value; }
        }

        /// <summary>
        /// Gets or sets the Y-axis location of the pattern.
        /// </summary>
        [SvgAttribute("y")]
        public SvgUnit Y
        {
            get { return _y; }
            set { _y = value; Attributes["y"] = value; }
        }

        /// <summary>
        /// Gets or sets the width of the pattern.
        /// </summary>
        [SvgAttribute("width")]
        public SvgUnit Width
        {
            get { return _width; }
            set { _width = value; Attributes["width"] = value; }
        }

        /// <summary>
        /// Gets or sets the height of the pattern.
        /// </summary>
        [SvgAttribute("height")]
        public SvgUnit Height
        {
            get { return _height; }
            set { _height = value; Attributes["height"] = value; }
        }

        /// <summary>
        /// Gets or sets the width of the pattern.
        /// </summary>
        [SvgAttribute("patternUnits")]
        public SvgCoordinateUnits PatternUnits
        {
            get { return _patternUnits; }
            set { _patternUnits = value; Attributes["patternUnits"] = value; }
        }

        /// <summary>
        /// Gets or sets the width of the pattern.
        /// </summary>
        [SvgAttribute("patternContentUnits")]
        public SvgCoordinateUnits PatternContentUnits
        {
            get { return _patternContentUnits; }
            set { _patternContentUnits = value; Attributes["patternContentUnits"] = value; }
        }

        /// <summary>
        /// Specifies a supplemental transformation which is applied on top of any 
        /// transformations necessary to create a new pattern coordinate system.
        /// </summary>
        [SvgAttribute("viewBox")]
        public SvgViewBox ViewBox
        {
            get { return _viewBox; }
            set { _viewBox = value; Attributes["viewBox"] = value; }
        }

        /// <summary>
        /// Gets or sets another gradient fill from which to inherit the stops from.
        /// </summary>
        [SvgAttribute("href", SvgAttributeAttribute.XLinkNamespace)]
        public SvgDeferredPaintServer InheritGradient
        {
            get { return GetAttribute<SvgDeferredPaintServer>("href", false); }
            set { Attributes["href"] = value; }
        }

        [SvgAttribute("overflow")]
        public SvgOverflow Overflow
        {
            get { return GetAttribute<SvgOverflow>("overflow", false); }
            set { Attributes["overflow"] = value; }
        }

        /// <summary>
        /// Gets or sets the aspect of the viewport.
        /// </summary>
        /// <value></value>
        [SvgAttribute("preserveAspectRatio")]
        public SvgAspectRatio AspectRatio
        {
            get { return GetAttribute("preserveAspectRatio", false, new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid)); }
            set { Attributes["preserveAspectRatio"] = value; }
        }

        [SvgAttribute("patternTransform")]
        public SvgTransformCollection PatternTransform
        {
            get { return GetAttribute<SvgTransformCollection>("patternTransform", false); }
            set { Attributes["patternTransform"] = value; }
        }

        private Matrix3x2 EffectivePatternTransform
        {
            get
            {
                var transform = new Matrix3x2();
                if (PatternTransform != null)
                {
                    var matrix = PatternTransform.GetMatrix();
                    transform = Matrix3x2.Multiply(transform, matrix);
                }
                return transform;
            }
        }

        /// <summary>
        /// Gets a <see cref="Color"/> representing the current paint server.
        /// </summary>
        /// <param name="renderingElement">The owner <see cref="SvgVisualElement"/>.</param>
        /// <param name="renderer">The renderer object.</param>
        /// <param name="opacity">The opacity of the brush.</param>
        /// <param name="forStroke">Not used.</param>
        public override Color GetColor(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            return System.Drawing.Color.Black;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgPatternServer>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgPatternServer;

            newObj._x = _x;
            newObj._y = _y;
            newObj._width = _width;
            newObj._height = _height;
            newObj._patternUnits = _patternUnits;
            newObj._patternContentUnits = _patternContentUnits;
            newObj._viewBox = _viewBox;
            return newObj;
        }
    }
}
