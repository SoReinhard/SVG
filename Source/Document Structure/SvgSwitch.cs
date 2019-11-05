namespace Svg
{
    /// <summary>
    /// The 'switch' element evaluates the 'requiredFeatures', 'requiredExtensions' and 'systemLanguage' attributes on its direct child elements in order, and then processes and renders the first child for which these attributes evaluate to true
    /// </summary>
    [SvgElement("switch")]
    public class SvgSwitch : SvgVisualElement
    {
        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        /// <value></value>
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            return GetPaths(this, renderer);
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            if (!Visible || !Displayable)
                return;

            try
            {
                if (PushTransforms(renderer))
                {
                    base.RenderChildren(renderer);
                }
            }
            finally
            {
                PopTransforms(renderer);
            }
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgSwitch>();
        }
    }
}
