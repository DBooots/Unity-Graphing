using System;
using UnityEngine;

namespace Graphing
{
    /// <summary>
    /// A class that maps input values to a <see cref="Color"/>.
    /// </summary>
    public sealed class ColorMap
    {
        /// <summary>
        /// Visually similar to MATLAB's Jet color map.
        /// </summary>
        public static readonly ColorMap Jet = new ColorMap(ColorMapJet);
        /// <summary>
        /// Visually similar to MATLAB's Jet color map but with deeper blues.
        /// </summary>
        public static readonly ColorMap Jet_Dark = new ColorMap(ColorMapJetDark);

        /// <summary>
        /// A <see cref="delegate"/> that maps from a <see cref="float"/> value to a <see cref="Color"/>.
        /// </summary>
        /// <param name="value">The input value.</param>
        /// <returns>The corresponding mapped <see cref="Color"/>.</returns>
        public delegate Color ColorMapDelegate(float value);
        private ColorMapDelegate colormapDelegate;
        /// <summary>
        /// A <see cref="Predicate{T}"/> that forces failing values to be drawn as a specified color (usually <see cref="Color.clear"/>.
        /// </summary>
        public Predicate<float> Filter { get; set; } = (v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private Color FilterColor { get; set; } = Color.clear;
        private bool useFunc = false;
        private bool Stepped { get; set; } = false;
        private Color[] colors;
        private int count;

        /// <summary>
        /// Constructs a new <see cref="ColorMap"/> with evenly spaced <see cref="Color"/>s.
        /// </summary>
        /// <param name="colors">An array of the <see cref="Color"/>s to be used, from lowest to highest.</param>
        public ColorMap(params Color[] colors)
        {
            this.colors = colors;
            this.useFunc = false;
            this.count = colors.Length;
        }
        /// <summary>
        /// Constructs a new <see cref="ColorMap"/> using the provided <see cref="ColorMapDelegate"/>.
        /// </summary>
        /// <param name="colorMapFunction">The desired <see cref="ColorMapDelegate"/></param>.
        public ColorMap(ColorMapDelegate colorMapFunction)
        {
            this.colormapDelegate = colorMapFunction;
            this.useFunc = true;
        }
        /// <summary>
        /// Constructs a new <see cref="ColorMap"/> based on an existing one.
        /// </summary>
        /// <param name="colorMap">The base <see cref="ColorMap"/>.</param>
        public ColorMap(ColorMap colorMap)
        {
            this.colormapDelegate = colorMap.colormapDelegate;
            this.Filter = colorMap.Filter;
            this.FilterColor = colorMap.FilterColor;
            this.useFunc = colorMap.useFunc;
            this.Stepped = colorMap.Stepped;
            this.colors = colorMap.colors;
            this.count = colorMap.count;
        }

        /// <summary>
        /// Gets the <see cref="Color"/> mapped to by the given value.
        /// </summary>
        /// <param name="value">The value to be mapped.</param>
        /// <returns>The corresponding <see cref="Color"/>.</returns>
        public Color this[float value]
        {
            get
            {
                if (!Filter(value))
                    return FilterColor;
                if (useFunc)
                    return colormapDelegate(Mathf.Clamp01(value));
                if (count == 1)
                    return colors[0];
                int index = Mathf.FloorToInt(Mathf.Clamp01(value) * count);
                if (index >= 1)
                    return colors[count - 1];
                if (index <= 0)
                    return colors[0];
                if (Stepped)
                    return colors[index];
                return Color.Lerp(colors[index], colors[index + 1], Mathf.Clamp01(value) * count % 1);
            }
        }

        /// <summary>
        /// Returns <see cref="this[float]"/> with an input value of 0.
        /// </summary>
        /// <param name="colorMap"></param>
        public static explicit operator Color(ColorMap colorMap)
        {
            return colorMap[0];
        }
        /// <summary>
        /// Creates a new <see cref="ColorMap"/> with a single <see cref="Color"/>.
        /// </summary>
        /// <param name="color"></param>
        public static implicit operator ColorMap(Color color)
        {
            return new ColorMap(color);
        }

        private static Color ColorMapJet(float value)
        {
            if (float.IsNaN(value))
                return Color.black;

            const float fractional = 1f / 3f;
            const float mins = 128f / 255f;

            if (value < fractional)
            {
                value = (value / fractional * (128 - 255) + 255) / 255;
                return new Color(mins, 1, value, 1);
            }
            if (value < 2 * fractional)
            {
                value = ((value - fractional) / fractional * (255 - 128) + 128) / 255;
                return new Color(value, 1, mins, 1);
            }
            value = ((value - 2 * fractional) / fractional * (128 - 255) + 255) / 255;
            return new Color(1, value, mins, 1);
        }
        private static Color ColorMapJetDark(float value)
        {
            if (float.IsNaN(value))
                return Color.black;

            const float fractional = 0.25f;
            const float mins = 128f / 255f;

            if (value < fractional)
            {
                value = (value / fractional * (255 - 128) + 128) / 255;
                return new Color(mins, value, 1, 1);
            }
            if (value < 2 * fractional)
            {
                value = ((value - fractional) / fractional * (128 - 255) + 255) / 255;
                return new Color(mins, 1, value, 1);
            }
            if (value < 3 * fractional)
            {
                value = ((value - 2 * fractional) / fractional * (255 - 128) + 128) / 255;
                return new Color(value, 1, mins, 1);
            }
            value = ((value - 3 * fractional) / fractional * (128 - 255) + 255) / 255;
            return new Color(1, value, mins, 1);
        }
    }
}
