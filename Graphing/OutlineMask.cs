using System;
using UnityEngine;

namespace Graphing
{
    /// <summary>
    /// A class representing a contour of a surface graph.
    /// </summary>
    public class OutlineMask : Graphable3
    {
        /// <summary>
        /// Defines the <see cref="ColorMap"/> for the graph.
        /// </summary>
        public override ColorMap Color { get; set; } = UnityEngine.Color.gray;

        public Func<float, bool> MaskCriteria { get; set; } = (v) => !float.IsNaN(v) && !float.IsInfinity(v) && v >= 0;
        public bool LineOnly { get; set; } = true;
        public int LineWidth { get; set; } = 1;
        public bool ForceClear { get; set; } = false;

        protected float[,] _values;
        public float[,] Values
        {
            get { return _values; }
            set
            {
                _values = value;
                OnValuesChanged(null);
            }
        }

        public OutlineMask(float[,] values, float xLeft, float xRight, float yBottom, float yTop, Func<float, bool> maskCriteria = null)
        {
            this._values = values;
            this.xMin = xLeft;
            this.xMax = xRight;
            this.yMin = yBottom;
            this.yMax = yTop;
            if (maskCriteria != null)
                this.MaskCriteria = maskCriteria;
        }

        /// <summary>
        /// Draws the object on the specified <see cref="UnityEngine.Texture2D"/>.
        /// </summary>
        /// <param name="texture">The texture on which to draw the object.</param>
        /// <param name="xLeft">The X axis lower bound.</param>
        /// <param name="xRight">The X axis upper bound.</param>
        /// <param name="yBottom">The Y axis lower bound.</param>
        /// <param name="yTop">The Y axis upper bound.</param>
        public override void Draw(ref Texture2D texture, float xLeft, float xRight, float yBottom, float yTop)
        {
            if (!Visible) return;
            int width = texture.width - 1;
            int height = texture.height - 1;

            float graphStepX = (xRight - xLeft) / width;
            float graphStepY = (yTop - yBottom) / height;

            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    float xF = x * graphStepX + xLeft;
                    float yF = y * graphStepY + yBottom;

                    float pixelValue = ValueAt(xF, yF);

                    if (LineOnly)
                    {
                        bool mask = false;

                        if (!MaskCriteria(pixelValue))
                        {
                            for (int w = 1; w <= LineWidth; w++)
                            {
                                if ((x >= w && MaskCriteria(ValueAt((x - w) * graphStepX + xLeft, yF))) ||
                                    (x < width - w && MaskCriteria(ValueAt((x + w) * graphStepX + xLeft, yF))) ||
                                    (y >= w && MaskCriteria(ValueAt(xF, (y - w) * graphStepY + yBottom))) ||
                                    (y < height - w && MaskCriteria(ValueAt(xF, (y + w) * graphStepY + yBottom))))
                                {
                                    mask = true;
                                    break;
                                }
                            }
                        }
                        if (mask)
                            texture.SetPixel(x, y, Color[ColorFunc(x, y, pixelValue)]);
                        else if (ForceClear)
                            texture.SetPixel(x, y, UnityEngine.Color.clear);
                    }
                    else
                    {
                        if (!MaskCriteria(pixelValue) || xF < XMin || xF > XMax || yF < YMin || yF > YMax)
                            texture.SetPixel(x, y, Color[ColorFunc(xF, yF, pixelValue)]);
                        else if (ForceClear)
                            texture.SetPixel(x, y, UnityEngine.Color.clear);
                    }
                }
            }

            texture.Apply();
        }

        /// <summary>
        /// Gets a formatted value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="withName">When true, requests the object include its name.</param>
        /// <returns></returns>
        public override string GetFormattedValueAt(float x, float y, bool withName = false)
        {
            return "";
        }

        /// <summary>
        /// Gets a value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <returns></returns>
        public override float ValueAt(float x, float y)
        {
            if (Transpose)
            {
                float temp = x;
                x = y;
                y = temp;
            }

            int xI1, xI2;
            float fX;
            if (x <= XMin)
            {
                xI1 = xI2 = 0;
                fX = 0;
            }
            else
            {
                int lengthX = _values.GetUpperBound(0);
                if (lengthX < 0)
                    return 0;
                if (x >= XMax)
                {
                    xI1 = xI2 = lengthX;
                    fX = 1;
                }
                else
                {
                    float stepX = (XMax - XMin) / lengthX;
                    xI1 = (int)Math.Floor((x - XMin) / stepX);
                    fX = (x - XMin) / stepX % 1;
                    xI2 = xI1 + 1;
                    if (fX == 0)
                        xI2 = xI1;
                    else
                        xI2 = xI1 + 1;
                }
            }

            if (y <= YMin)
            {
                if (xI1 == xI2) return _values[xI1, 0];
                return _values[xI1, 0] * (1 - fX) + _values[xI2, 0] * fX;
            }
            else
            {
                int lengthY = _values.GetUpperBound(1);
                if (lengthY < 0)
                    return 0;
                if (y >= YMax)
                {
                    if (xI1 == xI2) return _values[xI1, 0];
                    return _values[xI1, lengthY] * (1 - fX) + _values[xI2, lengthY] * fX;
                }
                else
                {
                    float stepY = (YMax - YMin) / lengthY;
                    int yI1 = (int)Math.Floor((y - YMin) / stepY);
                    float fY = (y - YMin) / stepY % 1;
                    int yI2;
                    if (fY == 0)
                        yI2 = yI1;
                    else
                        yI2 = yI1 + 1;

                    if (xI1 == xI2 && yI1 == yI2)
                        return _values[xI1, yI1];
                    else if (xI1 == xI2)
                        return _values[xI1, yI1] * (1 - fY) + _values[xI1, yI2] * fY;
                    else if (yI1 == yI2)
                        return _values[xI1, yI1] * (1 - fX) + _values[xI2, yI1] * fX;

                    return _values[xI1, yI1] * (1 - fX) * (1 - fY) +
                        _values[xI2, yI1] * fX * (1 - fY) +
                        _values[xI1, yI2] * (1 - fX) * fY +
                        _values[xI2, yI2] * fX * fY;
                }
            }
        }

        public void SetValues(float[,] values, float xLeft, float xRight, float yBottom, float yTop)
        {
            this._values = values;
            this.XMin = xLeft;
            this.XMax = xRight;
            this.YMin = yBottom;
            this.YMax = yTop;
            OnValuesChanged(null);
        }

        /// <summary>
        /// Outputs the object's values to file.
        /// </summary>
        /// <param name="directory">The directory in which to place the file.</param>
        /// <param name="filename">The filename for the file.</param>
        /// <param name="sheetName">An optional sheet name for within the file.</param>
        public override void WriteToFile(string directory, string filename, string sheetName = "")
        {
            return;
        }
    }
}
