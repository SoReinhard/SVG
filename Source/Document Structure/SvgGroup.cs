using System.Drawing;

namespace Svg
{
    /// <summary>
    /// An element used to group SVG shapes.
    /// </summary>
    [SvgElement("g")]
    public class SvgGroup : SvgMarkerElement
    {
        bool markersSet = false;

        /// <summary>
        /// If the group has marker attributes defined, add them to all children
        /// that are able to display markers. Only done once.
        /// </summary>
        private void AddMarkers()
        {
            if (!markersSet)
            {
                if (this.MarkerStart != null || this.MarkerMid != null || this.MarkerEnd != null)
                {
                    foreach (var c in this.Children)
                    {
                        if (c is SvgMarkerElement)
                        {
                            if (this.MarkerStart != null && ((SvgMarkerElement)c).MarkerStart == null)
                            {
                                ((SvgMarkerElement)c).MarkerStart = this.MarkerStart;
                            }
                            if (this.MarkerMid != null && ((SvgMarkerElement)c).MarkerMid == null)
                            {
                                ((SvgMarkerElement)c).MarkerMid = this.MarkerMid;
                            }
                            if (this.MarkerEnd != null && ((SvgMarkerElement)c).MarkerEnd == null)
                            {
                                ((SvgMarkerElement)c).MarkerEnd = this.MarkerEnd;
                            }
                        }
                    }
                }
                markersSet = true;
            }
        }

        /// <summary>
        /// Add group markers to children before rendering them.
        /// This is only done on first rendering.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to render the child <see cref="SvgElement"/>s to.</param>
        protected override void Render(ISvgRenderer renderer)
        {
            AddMarkers();
            base.Render(renderer);
        }

        /// <summary>
        /// Gets the <see cref="ISvgRenderer"/> for this element.
        /// </summary>
        /// <value></value>
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            return GetPaths(this, renderer);
        }

        protected override bool Renderable { get { return false; } }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgGroup>();
        }
    }
}
