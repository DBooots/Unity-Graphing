using System.Collections.Generic;
using UnityEngine;

namespace Graphing
{
    /// <summary>
    /// A struct representing a graph axis.
    /// </summary>
    public struct Axis
    {
        /// <summary>
        /// The tick mark labels.
        /// </summary>
        public List<string> labels;
        /// <summary>
        /// The number of tick marks.
        /// </summary>
        public int TickCount { get; private set; }
        /// <summary>
        /// The major unit (tick mark step size).
        /// </summary>
        public float MajorUnit { get; private set; }
        private float _min, _max;
        private bool forX;
        /// <summary>
        /// Get or set the axis lower bound.
        /// </summary>
        public float Min
        {
            get { return _min; }
            set
            {
                CalculateBounds(value, Max, forX);
            }
        }
        /// <summary>
        /// Get or set the axis upper bound.
        /// </summary>
        public float Max
        {
            get { return _max; }
            set
            {
                CalculateBounds(Min, value, forX);
            }
        }

        /// <summary>
        /// Construct a new axis.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="forX">Bool representing if this axis is for the X-axis.</param>
        /// <param name="forceBounds">When true, forces the bounds to be equal to the minimum 
        /// and maximum values rather than rounded to the nearest major unit.</param>
        public Axis(float min, float max, bool forX = true, bool forceBounds = false)
        {
            // I have to initialize the fields before calling the proper
            // initialization method...
            _min = _max = 0;
            MajorUnit = 0;
            TickCount = 0;
            this.labels = new List<string>();
            this.forX = forX;
            CalculateBounds(min, max, forX, forceBounds);
        }

        private void CalculateBounds(float min, float max, bool forX = true, bool forceBounds = false)
        {
            if (min > max)
            {
                float temp = min;
                min = max;
                max = temp;
            }
            if (min == max || float.IsNaN(min) || float.IsNaN(max) || float.IsInfinity(min) || float.IsInfinity(max))
            {
                this._min = min;
                this._max = max;
                this.MajorUnit = 0;
                this.TickCount = 1;
                this.labels = new List<string>() { string.Format("{0}", this.Min), string.Format("{0}", this.Max) };
                return;
            }
            this.MajorUnit = GetMajorUnit(min, max, forX);
            if (!forceBounds || (min % MajorUnit == 0 && max % MajorUnit == 0))
            {
                if (min % MajorUnit == 0)
                    this._min = min;
                else
                    this._min = Mathf.Floor(Mathf.Min(min, 0) / MajorUnit * 1.05f) * MajorUnit;
                if (max % MajorUnit == 0)
                    this._max = max;
                else
                    this._max = Mathf.Ceil(Mathf.Max(max, 0) / MajorUnit * 1.05f) * MajorUnit;
            }
            else
            {
                this.MajorUnit = (max - min) / 10f;
                this._min = min;
                this._max = max;
            }

            TickCount = Mathf.RoundToInt((_max - _min) / MajorUnit);

            this.labels = new List<string>(TickCount + 1);
            for (int i = 0; i <= TickCount; i++)
                labels.Add(string.Format("{0}", _min + MajorUnit * i));
        }

        /// <summary>
        /// Draw the axis and tick marks on a <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="axisTex">The texture object to draw the axis on.</param>
        /// <param name="color">The color for the axis and tick marks.</param>
        /// <param name="clearBG">When true, sets the background to <see cref="Color.clear"/></param>.
        /// <param name="inverted">When true, draws the axis along the bottom or left rather 
        /// than the top or right.</param>
        public void DrawAxis(ref Texture2D axisTex, Color color, bool clearBG = true, bool inverted = false)
        {
            int width = axisTex.width - 1;
            int height = axisTex.height - 1;
            bool horizontal = height <= width;
            int major = horizontal ? width : height;
            int minor = horizontal ? height : width;
            int nextline = 0;
            int index = 0;

            for (int a = 0; a <= major; a++)
            {
                Color rowColor;
                bool colorLine = false;
                if (a == nextline)
                {
                    colorLine = true;
                    rowColor = color;
                    index++;
                    if (TickCount == 0)
                        nextline = major + 1;
                    else
                        nextline = Mathf.RoundToInt((float)major / TickCount * index);
                }
                else
                    rowColor = Color.clear;
                if (clearBG || colorLine)
                {
                    for (int b = 0; b <= minor; b++)
                    {
                        if (horizontal)
                            axisTex.SetPixel(a, b, rowColor);
                        else
                            axisTex.SetPixel(b, a, rowColor);
                    }
                }
                if (horizontal)
                    axisTex.SetPixel(a, inverted ? 0 : minor, color);
                else
                    axisTex.SetPixel(inverted ? 0 : minor, a, color);
            }

            axisTex.Apply();
        }

        /// <summary>
        /// Gets the major unit for a given minimum and maximum value.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="forX">Bool representing if this axis is for the X-axis.</param>
        /// <returns>The major unit.</returns>
        public static float GetMajorUnit(float min, float max, bool forX = true)
        {
            if (Mathf.Sign(max) != Mathf.Sign(min))
                return GetMajorUnit(max - min);
            float c;
            if (forX)
                c = 12f / 7;
            else
                c = 40f / 21;
            float range = Mathf.Max(max, -min);
            if (range < 0)
                range = -range;
            float oom = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(range)));
            float normVal = range / oom;
            if (normVal > 5 * c)
                return 2 * oom;
            else if (normVal > 2.5f * c)
                return oom;
            else if (normVal > c)
                return 0.5f * oom;
            else
                return 0.2f * oom;
        }
        /// <summary>
        /// Gets the major unit for a given range.
        /// </summary>
        /// <param name="range">The distance from the upper bound to the lower bound.</param>
        /// <returns>The major unit.</returns>
        public static float GetMajorUnit(float range)
        {
            const float c = 18f / 11;
            if (range < 0)
                range = -range;
            float oom = Mathf.Pow(10, Mathf.Floor(Mathf.Log10(range)));
            float normVal = range / oom;
            if (normVal > 5 * c)
                return 2 * oom;
            else if (normVal > 2.5f * c)
                return oom;
            else if (normVal > c)
                return 0.5f * oom;
            else
                return 0.2f * oom;
        }
        /// <summary>
        /// Gets the upper bound for a given minimum and maximum value.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="forX">Bool representing if this axis is for the X-axis.</param>
        /// <returns></returns>
        public static float GetMax(float min, float max, bool forX = true)
        {
            if (min > max)
            {
                float temp = min;
                min = max;
                max = temp;
            }
            float majorUnit = GetMajorUnit(min, max, forX);
            if (max % majorUnit == 0)
                return max;
            else
                return Mathf.Ceil(Mathf.Max(max, 0) / majorUnit * 1.05f) * majorUnit;
        }
        /// <summary>
        /// Gets the lower bound for a given minimum and maximum value.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="forX">Bool representing if this axis is for the X-axis.</param>
        /// <returns>The lower bound.</returns>
        public static float GetMin(float min, float max, bool forX = true)
        {
            if (min > max)
            {
                float temp = min;
                min = max;
                max = temp;
            }
            float majorUnit = GetMajorUnit(min, max, forX);
            if (min % majorUnit == 0)
                return min;
            else
                return Mathf.Floor(Mathf.Min(min, 0) / majorUnit * 1.05f) * majorUnit;
        }
    }
}
