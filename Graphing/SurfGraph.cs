using System;
using Graphing.Extensions;

namespace Graphing
{
    /// <summary>
    /// A class representing a surface graph.
    /// </summary>
    public class SurfGraph : Graphable3
    {
        /// <summary>
        /// Defines the <see cref="ColorMap"/> for the graph.
        /// </summary>
        public override ColorMap Color { get; set; } = ColorMap.Jet_Dark;

        /// <summary>
        /// The color axis lower bound.
        /// </summary>
        public float CMin { get; set; } = float.NaN;
        /// <summary>
        /// The color axis upper bound.
        /// </summary>
        public float CMax { get; set; } = float.NaN;
        
        protected float[,] _values;
        /// <summary>
        /// Gets or sets the points in the surface.
        /// </summary>
        public float[,] Values
        {
            get { return _values; }
            set
            {
                _values = value;
                OnValuesChanged(null);
            }
        }

        /// <summary>
        /// Constructs a blank <see cref="SurfGraph"/>.
        /// </summary>
        public SurfGraph() { this.ColorFunc = (x, y, z) => z; }
        /// <summary>
        /// Constructs a <see cref="SurfGraph"/> with the specified values spaced evenly on a grid.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="xLeft">The left x bound.</param>
        /// <param name="xRight">The right x bound.</param>
        /// <param name="yBottom">The bottom y bound.</param>
        /// <param name="yTop">The top y bound.</param>
        public SurfGraph(float[,] values, float xLeft, float xRight, float yBottom, float yTop) : this()
        {
            this._values = values;
            this.XMin = xLeft;
            this.XMax = xRight;
            this.YMin = yBottom;
            this.YMax = yTop;
            if (_values.GetUpperBound(0) < 0 || _values.GetUpperBound(1) < 0)
            {
                ZMin = ZMax = 0;
                return;
            }
            this.ZMin = values.Min(true);
            this.ZMax = values.Max(true);
        }

        /// <summary>
        /// Draws the object on the specified <see cref="UnityEngine.Texture2D"/>.
        /// </summary>
        /// <param name="texture">The texture on which to draw the object.</param>
        /// <param name="xLeft">The X axis lower bound.</param>
        /// <param name="xRight">The X axis upper bound.</param>
        /// <param name="yBottom">The Y axis lower bound.</param>
        /// <param name="yTop">The Y axis upper bound.</param>
        public override void Draw(ref UnityEngine.Texture2D texture, float xLeft, float xRight, float yBottom, float yTop)
            => this.Draw(ref texture, xLeft, xRight, yBottom, yTop, ZMin, ZMax);

        /// <summary>
        /// Draws the object on the specified <see cref="UnityEngine.Texture2D"/>.
        /// </summary>
        /// <param name="texture">The texture on which to draw the object.</param>
        /// <param name="xLeft">The X axis lower bound.</param>
        /// <param name="xRight">The X axis upper bound.</param>
        /// <param name="yBottom">The Y axis lower bound.</param>
        /// <param name="yTop">The Y axis upper bound.</param>
        /// <param name="cMin">The color axis lower bound.</param>
        /// <param name="cMax">The color axis upper bound.</param>
        public void Draw(ref UnityEngine.Texture2D texture, float xLeft, float xRight, float yBottom, float yTop, float cMin, float cMax)
        {
            if (!Visible) return;
            int width = texture.width - 1;
            int height = texture.height - 1;
            
            float graphStepX = (xRight - xLeft) / width;
            float graphStepY = (yTop - yBottom) / height;
            float cRange = cMax - cMin;

            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    float xF = x * graphStepX + xLeft;
                    float yF = y * graphStepY + yBottom;
                    if (xF < XMin || xF > XMax || yF < YMin || yF > YMax)
                        continue;
                    texture.SetPixel(x, y, this.Color[(ColorFunc(xF, yF, ValueAt(xF, yF)) - cMin) / cMax]);
                }
            }

            texture.Apply();
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

        /// <summary>
        /// Draws an equivalent to <see cref="OutlineMask"/>.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="xLeft"></param>
        /// <param name="xRight"></param>
        /// <param name="yBottom"></param>
        /// <param name="yTop"></param>
        /// <param name="maskCriteria"></param>
        /// <param name="maskColor"></param>
        /// <param name="lineOnly"></param>
        /// <param name="lineWidth"></param>
        public void DrawMask(ref UnityEngine.Texture2D texture, float xLeft, float xRight, float yBottom, float yTop, Func<float, bool> maskCriteria, UnityEngine.Color maskColor, bool lineOnly = true, int lineWidth = 1)
        {
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

                    if (lineOnly)
                    {
                        float pixelValue = ValueAt(xF, yF);
                        bool mask = false;

                        if (!maskCriteria(pixelValue))
                        {
                            for (int w = 1; w <= lineWidth; w++)
                            {
                                if ((x >= w && maskCriteria(ValueAt((x - w) * graphStepX + xLeft, yF))) ||
                                    (x < width - w && maskCriteria(ValueAt((x + w) * graphStepX + xLeft, yF))) ||
                                    (y >= w && maskCriteria(ValueAt(xF, (y - w) * graphStepY + yBottom))) ||
                                    (y < height - w && maskCriteria(ValueAt(xF, (y + w) * graphStepY + yBottom))))
                                {
                                    mask = true;
                                    break;
                                }
                            }
                        }
                        if (mask)
                            texture.SetPixel(x, y, maskColor);
                        else
                            texture.SetPixel(x, y, UnityEngine.Color.clear);
                    }
                    else
                    {
                        if (!maskCriteria(ValueAt(xF, yF)) || xF < XMin || xF > XMax || yF < YMin || yF > YMax)
                            texture.SetPixel(x, y, maskColor);
                        else
                            texture.SetPixel(x, y, UnityEngine.Color.clear);
                    }
                }
            }

            texture.Apply();
        }

        public void SetValues(float[,] values, float xLeft, float xRight, float yBottom, float yTop)
        {
            this._values = values;
            this.XMin = xLeft;
            this.XMax = xRight;
            this.YMin = yBottom;
            this.YMax = yTop;
            if (_values.GetUpperBound(0) < 0 || _values.GetUpperBound(1) < 0)
            {
                ZMin = ZMax = 0;
                return;
            }
            this.ZMin = values.Min(true);
            this.ZMax = values.Max(true);
            this.CMin = ZMin;
            this.CMax = CMax;
            OnValuesChanged(null);
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
            if (_values.GetUpperBound(0) < 0 || _values.GetUpperBound(1) < 0) return "";
            return base.GetFormattedValueAt(x, y, withName);
        }

        /// <summary>
        /// Outputs the object's values to file.
        /// </summary>
        /// <param name="directory">The directory in which to place the file.</param>
        /// <param name="filename">The filename for the file.</param>
        /// <param name="sheetName">An optional sheet name for within the file.</param>
        public override void WriteToFile(string directory, string filename, string sheetName = "")
        {
            int height = _values.GetUpperBound(1);
            int width = _values.GetUpperBound(0);
            if (height < 0 || width < 0)
                return;
            float xStep = (XMax - XMin) / width;
            float yStep = (YMax - YMin) / height;

            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            if (sheetName == "")
                sheetName = this.Name.Replace("/", "-").Replace("\\", "-");

            string fullFilePath = string.Format("{0}/{1}{2}.csv", directory, filename, sheetName != "" ? "_" + sheetName : "");

            try
            {
                if (System.IO.File.Exists(fullFilePath))
                    System.IO.File.Delete(fullFilePath);
            }
            catch (Exception ex) { UnityEngine.Debug.LogFormat("Unable to delete file:{0}", ex.Message); }
            
            string strCsv;
            if (Name != "")
                strCsv = String.Format("{0} [{1}]", Name, ZUnit != "" ? ZUnit : "-");
            else
                strCsv = String.Format("{0}", ZUnit != "" ? ZUnit : "-");

            for (int x = 0; x <= width; x++)
                strCsv += String.Format(",{0}", xStep * x + XMin);

            try
            {
                System.IO.File.AppendAllText(fullFilePath, strCsv + "\r\n");
            }
            catch (Exception) { }

            for (int y = height; y >= 0; y--)
            {
                strCsv = string.Format("{0}", y * yStep + YMin);
                for (int x = 0; x <= width; x++)
                    strCsv += string.Format(",{0:" + StringFormat.Replace("N", "F") + "}", _values[x, y]);

                try
                {
                    System.IO.File.AppendAllText(fullFilePath, strCsv + "\r\n");
                }
                catch (Exception) { }
            }
        }
    }
}
