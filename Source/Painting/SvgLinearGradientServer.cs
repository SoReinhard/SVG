using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Svg
{
    [SvgElement("linearGradient")]
    public sealed class SvgLinearGradientServer : SvgGradientServer
    {
        [SvgAttribute("x1")]
        public SvgUnit X1
        {
            get { return GetAttribute("x1", false, new SvgUnit(SvgUnitType.Percentage, 0f)); }
            set { Attributes["x1"] = value; }
        }

        [SvgAttribute("y1")]
        public SvgUnit Y1
        {
            get { return GetAttribute("y1", false, new SvgUnit(SvgUnitType.Percentage, 0f)); }
            set { Attributes["y1"] = value; }
        }

        [SvgAttribute("x2")]
        public SvgUnit X2
        {
            get { return GetAttribute("x2", false, new SvgUnit(SvgUnitType.Percentage, 100f)); }
            set { Attributes["x2"] = value; }
        }

        [SvgAttribute("y2")]
        public SvgUnit Y2
        {
            get { return GetAttribute("y2", false, new SvgUnit(SvgUnitType.Percentage, 0f)); }
            set { Attributes["y2"] = value; }
        }

        private bool IsInvalid
        {
            get
            {
                // Need at least 2 colours to do the gradient fill
                return this.Stops.Count < 2;
            }
        }

        public override Color GetColor(SvgVisualElement renderingElement, ISvgRenderer renderer, float opacity, bool forStroke = false)
        {
            LoadStops(renderingElement);

            if (this.Stops.Count < 1)
                return System.Drawing.Color.Black;

            var stopColor = this.Stops[0].GetColor(renderingElement);
            int alpha = (int)Math.Round((opacity * (stopColor.A / 255.0f)) * 255);
            Color colour = System.Drawing.Color.FromArgb(alpha, stopColor);
            return colour;
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgLinearGradientServer>();
        }
    }
}
