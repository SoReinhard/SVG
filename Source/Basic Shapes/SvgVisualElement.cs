using System;

namespace Svg
{
    /// <summary>
    /// The class that all SVG elements should derive from when they are to be rendered.
    /// </summary>
    public abstract partial class SvgVisualElement : SvgElement, ISvgStylable
    {
        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        public abstract GraphicsPath Path(ISvgRenderer renderer);

        /// <summary>
        /// Gets the associated Filter if one has been specified.
        /// </summary>
        [SvgAttribute("filter")]
        public virtual Uri Filter
        {
            get { return GetAttribute<Uri>("filter", true); }
            set { Attributes["filter"] = value; }
        }

        private bool ConvertShapeRendering2AntiAlias(SvgShapeRendering shapeRendering)
        {
            switch (shapeRendering)
            {
                case SvgShapeRendering.OptimizeSpeed:
                case SvgShapeRendering.CrispEdges:
                case SvgShapeRendering.GeometricPrecision:
                    return false;
                default:
                    // SvgShapeRendering.Auto
                    // SvgShapeRendering.Inherit
                    return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgVisualElement"/> class.
        /// </summary>
        public SvgVisualElement()
        {
            this.IsPathDirty = true;
        }

        protected virtual bool Renderable { get { return true; } }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            if (Visible && Displayable && (!Renderable || Path(renderer) != null))
            {
                try
                {
                    if (PushTransforms(renderer))
                    {
                        var opacity = FixOpacityValue(Opacity);
                        if (Renderable)
                            RenderFillAndStroke(renderer);
                        else
                            RenderChildren(renderer);
                    }
                }
                finally
                {
                    PopTransforms(renderer);
                }
            }
        }

        protected internal virtual void RenderFillAndStroke(ISvgRenderer renderer)
        {
            if (!RenderFill(renderer) && !RenderStroke(renderer))
            {
                RenderStroke(renderer, new SvgColourServer(System.Drawing.Color.Black));
            }
        }

        /// <summary>
        /// Renders the fill of the <see cref="SvgVisualElement"/> to the specified <see cref="ISvgRenderer"/>
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        /// <param name="fill"></param>
        protected internal virtual bool RenderFill(ISvgRenderer renderer, SvgPaintServer fill = null)
        {
            fill = fill ?? Fill;

            if (fill != null && fill != SvgPaintServer.None)
            {
                renderer.FillPath(Path(renderer), fill.GetColor(this, renderer, FixOpacityValue(FillOpacity), false));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Renders the stroke of the <see cref="SvgVisualElement"/> to the specified <see cref="ISvgRenderer"/>
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        /// <param name="stroke"></param>
        protected internal virtual bool RenderStroke(ISvgRenderer renderer, SvgPaintServer stroke = null)
        {
            stroke = stroke ?? Stroke;

            if (stroke != null && stroke != SvgPaintServer.None)
            {
                var strokeWidth = StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);

                renderer.DrawPath(Path(renderer), stroke.GetColor(this, renderer, FixOpacityValue(StrokeOpacity), true), strokeWidth);
                return true;
            }

            return false;
        }
    }
}
