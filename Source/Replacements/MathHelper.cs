using System;
using System.Collections.Generic;
using System.Text;

namespace Svg
{
    public static class MathHelper
    {
        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const float DegToRad = (float)Math.PI / 180.0f;

        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const float RadToDeg = 180.0f / (float)Math.PI;
    }
}
