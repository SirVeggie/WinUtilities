using System;
using System.Collections.Generic;
using System.Text;

namespace WinUtilities {
    internal static class Matht {

        #region clamping
        public static double Clamp(double value, double min, double max) => Math.Min(Math.Max(value, min), max);
        public static float Clamp(float value, float min, float max) => Math.Min(Math.Max(value, min), max);
        public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);

        /// <summary></summary>
        /// <param name="max">Max is inclusive.</param>
        public static int CyclicalClampInclusive(int x, int min, int max) => FloorToInt(CyclicalClampInclusive((double)x, min, max));
        /// <summary></summary>
        /// <param name="max">Max is inclusive.</param>
        public static double CyclicalClampInclusive(double x, double min, double max) {
            if (min > max) {
                throw new ArgumentException("Min can't be bigger than max");
            }

            double delta = Math.Abs(x - Clamp(x, min, max));
            double range = max - min;

            if (range == 0) {
                return max;
            } else if (x > max) {
                return min + (((delta - 1) % range) + 1);
            } else if (x < min) {
                return max - (((delta - 1) % range) + 1);
            } else {
                return x;
            }
        }

        public static int CyclicalClamp(int x, int min, int max) => FloorToInt(CyclicalClamp((double)x, min, max));
        public static double CyclicalClamp(double x, double min, double max) {
            if (min == max)
                return min;
            if (min > max)
                throw new ArgumentException("Min can't be bigger than max");
            double range = max - min;
            return (((x - min) % range) + range) % range + min;
        }
        #endregion

        public static int RoundToInt(double value) => (int)Math.Round(value);
        public static int FloorToInt(double value) => (int)Math.Floor(value);
        public static int CeilToInt(double value) => (int)Math.Ceiling(value);
        public static double Percentage(double value, double min, double max) => (value - min) / (max - min);
        /// <summary>Project <paramref name="value"/> from range [<paramref name="low"/>, <paramref name="high"/>] to a new range [<paramref name="newLow"/>, <paramref name="newHigh"/>]</summary>
        public static double ToRange(double value, double low, double high, double newLow, double newHigh) => newLow + (newHigh - newLow) * Percentage(value, low, high);
        /// <summary>Project <paramref name="value"/> from range [<paramref name="low"/>, <paramref name="high"/>] to a new range [0, 1] or vice versa if <paramref name="reverse"/></summary>
        public static double UnitRange(double value, double low, double high, bool reverse = false) => !reverse ? ToRange(value, low, high, 0, 1) : ToRange(value, 0, 1, low, high);

        #region trigonometry
        /// <summary>Converts degrees to radians.</summary>
        /// <param name="d">Angle in degrees</param>
        public static double Radians(double d) => Math.PI / 180 * d;

        /// <summary>Converts radians to degrees.</summary>
        /// <param name="r">Angle in radians</param>
        public static double Degrees(double r) => 180 / Math.PI * r;

        public static Coord LineIntersection(Coord startA, Coord endA, Coord startB, Coord endB) {
            // Line A represented as a1x + b1y = c1
            double a1 = endA.Y - startA.Y;
            double b1 = startA.X - endA.X;
            double c1 = a1 * startA.X + b1 * startA.Y;

            // Line B represented as a2x + b2y = c2
            double a2 = endB.Y - startB.Y;
            double b2 = startB.X - endB.X;
            double c2 = a2 * startB.X + b2 * startB.Y;

            double determinant = a1 * b2 - a2 * b1;

            if (determinant == 0) {
                // The lines are parallel. This is simplified
                // by returning a pair of FLT_MAX
                return Coord.Max;
            } else {
                double x = (b2 * c1 - b1 * c2) / determinant;
                double y = (a1 * c2 - a2 * c1) / determinant;
                return new Coord(x, y);
            }
        }
        #endregion
    }
}
