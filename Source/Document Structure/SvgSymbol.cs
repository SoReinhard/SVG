using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Svg.Document_Structure
{
    /// <summary>
    /// An element used to group SVG shapes.
    /// </summary>
    [SvgElement("symbol")]
    public class SvgSymbol : SvgVisualElement
    {
        /// <summary>
        /// Gets or sets the viewport of the element.
        /// </summary>
        /// <value></value>
        [SvgAttribute("viewBox")]
        public SvgViewBox ViewBox
        {
            get { return GetAttribute<SvgViewBox>("viewBox", false); }
            set { Attributes["viewBox"] = value; }
        }

        /// <summary>
        /// Gets or sets the aspect of the viewport.
        /// </summary>
        /// <value></value>
        [SvgAttribute("preserveAspectRatio")]
        public SvgAspectRatio AspectRatio
        {
            get { return GetAttribute<SvgAspectRatio>("preserveAspectRatio", false); }
            set { Attributes["preserveAspectRatio"] = value; }
        }

        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        /// <value></value>
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            return GetPaths(this, renderer);
        }

        protected override bool Renderable { get { return false; } }

        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to be transformed.</param>
        protected internal override bool PushTransforms(ISvgRenderer renderer)
        {
            if (!base.PushTransforms(renderer))
                return false;
            ViewBox.AddViewBoxTransform(AspectRatio, renderer, null);
            return true;
        }

        // Only render if the parent is set to a Use element
        protected override void Render(ISvgRenderer renderer)
        {
            if (_parent is SvgUse) base.Render(renderer);
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgSymbol>();
        }
    }
}
