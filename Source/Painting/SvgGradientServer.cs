using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Svg.Transforms;

namespace Svg
{
    /// <summary>
    /// Provides the base class for all paint servers that wish to render a gradient.
    /// </summary>
    public abstract class SvgGradientServer : SvgPaintServer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SvgGradientServer"/> class.
        /// </summary>
        internal SvgGradientServer()
        {
            Stops = new List<SvgGradientStop>();
        }

        /// <summary>
        /// Called by the underlying <see cref="SvgElement"/> when an element has been added to the
        /// 'Children' collection.
        /// </summary>
        /// <param name="child">The <see cref="SvgElement"/> that has been added.</param>
        /// <param name="index">An <see cref="int"/> representing the index where the element was added to the collection.</param>
        protected override void AddElement(SvgElement child, int index)
        {
            if (child is SvgGradientStop)
                Stops.Add((SvgGradientStop)child);

            base.AddElement(child, index);
        }

        /// <summary>
        /// Called by the underlying <see cref="SvgElement"/> when an element has been removed from the
        /// 'Children' collection.
        /// </summary>
        /// <param name="child">The <see cref="SvgElement"/> that has been removed.</param>
        protected override void RemoveElement(SvgElement child)
        {
            if (child is SvgGradientStop)
                Stops.Remove((SvgGradientStop)child);

            base.RemoveElement(child);
        }

        /// <summary>
        /// Gets the ramp of colors to use on a gradient.
        /// </summary>
        public List<SvgGradientStop> Stops { get; private set; }

        /// <summary>
        /// Specifies what happens if the gradient starts or ends inside the bounds of the target rectangle.
        /// </summary>
        [SvgAttribute("spreadMethod")]
        public SvgGradientSpreadMethod SpreadMethod
        {
            get { return GetAttribute("spreadMethod", false, SvgGradientSpreadMethod.Pad); }
            set { Attributes["spreadMethod"] = value; }
        }

        /// <summary>
        /// Gets or sets the coordinate system of the gradient.
        /// </summary>
        [SvgAttribute("gradientUnits")]
        public SvgCoordinateUnits GradientUnits
        {
            get { return GetAttribute("gradientUnits", false, SvgCoordinateUnits.ObjectBoundingBox); }
            set { Attributes["gradientUnits"] = value; }
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

        [SvgAttribute("gradientTransform")]
        public SvgTransformCollection GradientTransform
        {
            get { return GetAttribute<SvgTransformCollection>("gradientTransform", false); }
            set { Attributes["gradientTransform"] = value; }
        }

        /// <summary>
        /// Gets or sets the colour of the gradient stop.
        /// </summary>
        [SvgAttribute("stop-color")]
        [TypeConverter(typeof(SvgPaintServerFactory))]
        public SvgPaintServer StopColor
        {
            get { return GetAttribute<SvgPaintServer>("stop-color", false, new SvgColourServer(System.Drawing.Color.Black)); }
            set { Attributes["stop-color"] = value; }
        }

        /// <summary>
        /// Gets or sets the opacity of the gradient stop (0-1).
        /// </summary>
        [SvgAttribute("stop-opacity")]
        public float StopOpacity
        {
            get { return GetAttribute("stop-opacity", false, 1f); }
            set { Attributes["stop-opacity"] = FixOpacityValue(value); }
        }

        protected Matrix3x2 EffectiveGradientTransform
        {
            get
            {
                var transform = Matrix3x2.Identity;

                if (GradientTransform != null)
                {
                    var matrix = GradientTransform.GetMatrix();
                    transform = Matrix3x2.Multiply(transform, matrix);
                }

                return transform;
            }
        }

        protected void LoadStops(SvgVisualElement parent)
        {
            var core = SvgDeferredPaintServer.TryGet<SvgGradientServer>(InheritGradient, parent);
            if (Stops.Count == 0 && core != null)
                Stops.AddRange(core.Stops);
        }

        protected static double CalculateDistance(Vector2 first, Vector2 second)
        {
            return Math.Sqrt(Math.Pow(first.X - second.X, 2) + Math.Pow(first.Y - second.Y, 2));
        }

        protected static float CalculateLength(Vector2 vector)
        {
            return (float)Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
        }
    }
}
