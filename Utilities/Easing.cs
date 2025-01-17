using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwertyLauncher.Utilities
{
    internal class Easing
    {
        internal static float CubicInOut(float t, float totaltime, float min, float max)
        {
            max -= min;
            t /= totaltime / 2;
            if (t < 1) return max / 2 * t * t * t + min;

            t -= 2;
            return max / 2 * (t * t * t + 2) + min;
        }
    }
}
