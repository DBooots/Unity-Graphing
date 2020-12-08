﻿using System;
using System.Linq;

namespace Graphing
{
    /// <summary>
    /// A class representing a line graph.
    /// </summary>
    public class LineGraph : Graphable, ILineGraph
    {
        /// <summary>
        /// The width of the line in pixels.
        /// </summary>
        public int LineWidth { get; set; } = 1;
        protected UnityEngine.Vector2[] _values;
        /// <summary>
        /// Gets or sets the points in the line.
        /// </summary>
        public UnityEngine.Vector2[] Values
        {
            get { return _values; }
            private set
            {
                _values = value;
                if (_values.Length <= 0)
                {
                    YMin = YMax = XMin = XMax = 0;
                    return;
                }
                float xLeft = float.MaxValue;
                float xRight = float.MinValue;
                float yMin = float.MaxValue;
                float yMax = float.MinValue;
                for (int i = value.Length - 1; i >= 0; i--)
                {
                    if (!float.IsInfinity(value[i].x) && !float.IsNaN(value[i].x))
                    {
                        xLeft = Math.Min(xLeft, value[i].x);
                        xRight = Math.Max(xRight, value[i].x);
                    }
                    if (!float.IsInfinity(value[i].y) && !float.IsNaN(value[i].y))
                    {
                        yMin = Math.Min(yMin, value[i].y);
                        yMax = Math.Max(yMax, value[i].y);
                    }
                }
                this.XMax = xRight;
                this.XMin = xLeft;
                this.YMax = yMax;
                this.YMin = yMin;

                float step = (xRight - xLeft) / (value.Length - 1);
                this.sorted = true;
                this.equalSteps = true;
                for (int i = value.Length - 1; i >= 0; i--)
                {
                    if (equalSteps && _values[i].x != xLeft + step * i)
                        equalSteps = false;
                    if (sorted && i > 0 && _values[i].x < _values[i - 1].x)
                        sorted = false;
                    if (!equalSteps && !sorted)
                        break;
                }

                OnValuesChanged(null);
            }
        }
        protected bool sorted = false;
        protected bool equalSteps = false;

        /// <summary>
        /// Constructs a new <see cref="LineGraph"/> with the provided y-values at evenly spaced points.
        /// </summary>
        /// <param name="xLeft">The x-value at the leftmost point.</param>
        /// <param name="xRight">The x-value at the rightmost point.</param>
        /// <param name="values"></param>
        public LineGraph(float[] values, float xLeft, float xRight)
        {
            SetValues(values, xLeft, xRight);
        }
        /// <summary>
        /// Constructs a new <see cref="LineGraph"/> with the provided points.
        /// </summary>
        /// <param name="values"></param>
        public LineGraph(UnityEngine.Vector2[] values)
        {
            this.Values = values;
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
        {
            if (!Visible) return;
            float xRange = xRight - xLeft;
            float yRange = yTop - yBottom;
            int width = texture.width;
            int height = texture.height;
            int[] xPix, yPix;
            // TODO: Add robustness for NaNs and Infinities.
            if (!Transpose)
            {
                xPix = _values.Select(vect => UnityEngine.Mathf.RoundToInt((vect.x - xLeft) / xRange * width)).ToArray();
                yPix = _values.Select(vect => UnityEngine.Mathf.RoundToInt((vect.y - yBottom) / yRange * height)).ToArray();
            }
            else
            {
                xPix = _values.Select(vect => UnityEngine.Mathf.RoundToInt((vect.y - yBottom) / yRange * width)).ToArray();
                yPix = _values.Select(vect => UnityEngine.Mathf.RoundToInt((vect.x - xLeft) / xRange * height)).ToArray();
            }

            for (int i = _values.Length - 2; i >= 0; i--)
            {
                DrawingHelper.DrawLine(ref texture, xPix[i], yPix[i], xPix[i + 1], yPix[i + 1], this.Color[ColorFunc(xPix[i], yPix[i], 0)], this.Color[ColorFunc(xPix[i + 1], yPix[i + 1], 0)]);
                for (int w = 2; w <= LineWidth; w++)
                {
                    int l = w % 2 == 0 ? (-w) >> 1 : (w - 1) >> 1;
                    DrawingHelper.DrawLine(ref texture, xPix[i] + l, yPix[i], xPix[i + 1] + l, yPix[i + 1], this.Color[ColorFunc(xPix[i], yPix[i], 0)], this.Color[ColorFunc(xPix[i + 1], yPix[i + 1], 0)]);
                    DrawingHelper.DrawLine(ref texture, xPix[i], yPix[i] + l, xPix[i + 1], yPix[i + 1] + l, this.Color[ColorFunc(xPix[i], yPix[i], 0)], this.Color[ColorFunc(xPix[i + 1], yPix[i + 1], 0)]);
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
            => ValueAt(x, y, 1, 1);
        /// <summary>
        /// Gets a value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="width">The x domain of the graph.</param>
        /// <param name="height">The y range of the graph.</param>
        /// <returns></returns>
        public virtual float ValueAt(float x, float y, float width, float height)
        {
            if (_values.Length <= 0)
                return 0;

            if (Transpose) x = y;
            
            if(equalSteps && sorted)
            {
                if (x <= XMin) return _values[0].y;

                int length = _values.Length - 1;
                if (x >= XMax) return _values[length].y;

                float step = (XMax - XMin) / length;
                int index = (int)Math.Floor((x - XMin) / step);
                float f = (x - XMin) / step % 1;
                if (f == 0)
                    return _values[index].y;
                return _values[index].y * (1 - f) + _values[index + 1].y * f;
            }
            else
            {
                if (sorted)
                {
                    //if (x <= Values[0].x)
                    //    return Values[0].y;
                    if (x >= _values[_values.Length - 1].x)
                        return _values[_values.Length - 1].y;
                    for (int i = _values.Length - 2; i >= 0; i--)
                    {
                        if (x > _values[i].x)
                        {
                            float f = (x - _values[i].x) / (_values[i + 1].x - _values[i].x);
                            return _values[i].y * (1 - f) + _values[i + 1].y * f;
                        }
                    }
                    return _values[0].y;
                }
                else
                {
                    UnityEngine.Vector2 point = new UnityEngine.Vector2(x, y);
                    UnityEngine.Vector2 closestPoint = _values[0];

                    float currentDistance = float.PositiveInfinity;
                    int length = _values.Length;
                    for (int i = 0; i < length - 1 - 1; i++)
                    {

                        UnityEngine.Vector2 lineDir = (_values[i + 1] - _values[i]).normalized;
                        float distance = UnityEngine.Vector2.Dot(point - _values[i], lineDir);
                        UnityEngine.Vector2 closestPt = _values[i] + distance * lineDir;
                        if (distance <= 0)
                        {
                            closestPt = _values[i];

                        }
                        else if ((closestPt - _values[i]).sqrMagnitude > (_values[i + 1] - _values[i]).sqrMagnitude)//(distance * distance >= (_values[i + 1] - _values[i]).sqrMagnitude)
                        {
                            closestPt = _values[i + 1];

                        }
                        UnityEngine.Vector2 LocalTransform(UnityEngine.Vector2 vector) => new UnityEngine.Vector2(vector.x / width, vector.y / height);
                        
                        float ptDistance = LocalTransform(point - closestPt).sqrMagnitude;

                        if ( ptDistance < currentDistance)
                        {
                            currentDistance = ptDistance;
                            closestPoint = closestPt;
                        }
                    }
                    return closestPoint.y;
                }
            }
        }

        /// <summary>
        /// Sets the values in the line with the provided y-values at evenly spaced points.
        /// </summary>
        /// <param name="xLeft">The x-value at the leftmost point.</param>
        /// <param name="xRight">The x-value at the rightmost point.</param>
        /// <param name="values"></param>
        public void SetValues(float[] values, float xLeft, float xRight)
        {
            this._values = new UnityEngine.Vector2[values.Length];
            if (_values.Length <= 0)
            {
                YMin = YMax = XMin = XMax = 0;
                return;
            }
            float xStep = (xRight - xLeft) / (values.Length - 1);
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            for (int i = values.Length - 1; i >= 0; i--)
            {
                this._values[i] = new UnityEngine.Vector2(xLeft + xStep * i, values[i]);
                if (!float.IsInfinity(_values[i].y) && !float.IsNaN(_values[i].y))
                {
                    yMin = Math.Min(yMin, _values[i].y);
                    yMax = Math.Max(yMax, _values[i].y);
                }
            }
            this.XMax = xRight;
            this.XMin = xLeft;
            this.YMax = yMax;
            this.YMin = yMin;
            this.sorted = true;
            this.equalSteps = true;

            OnValuesChanged(null);
        }
        /// <summary>
        /// Sets the values in the line.
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(UnityEngine.Vector2[] values)
        {
            this.Values = values;
        }

        /// <summary>
        /// Gets a formatted value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="withName">When true, requests the object include its name.</param>
        /// <returns></returns>
        public override string GetFormattedValueAt(float x, float y, bool withName = false)
            => GetFormattedValueAt(x, y, 1, 1, withName);
        /// <summary>
        /// Gets a formatted value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="width">The x domain of the graph.</param>
        /// <param name="height">The y range of the graph.</param>
        /// <param name="withName">When true, requests the object include its name.</param>
        /// <returns></returns>
        public virtual string GetFormattedValueAt(float x, float y, float width, float height, bool withName = false)
        {
            if (_values.Length <= 0) return "";
            return String.Format("{2}{0:" + StringFormat + "}{1}", ValueAt(x, y, width, height), YUnit, withName && DisplayName != "" ? DisplayName + ": " : "");
        }

        /// <summary>
        /// Outputs the object's values to file.
        /// </summary>
        /// <param name="directory">The directory in which to place the file.</param>
        /// <param name="filename">The filename for the file.</param>
        /// <param name="sheetName">An optional sheet name for within the file.</param>
        public override void WriteToFile(string directory, string filename, string sheetName = "")
        {
            if (_values.Length <= 0)
                return;

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
            
            string strCsv = "";
            if (XName != "")
                strCsv += string.Format("{0} [{1}]", XName, XUnit != "" ? XUnit : "-");
            else
                strCsv += string.Format("{0}", XUnit != "" ? XUnit : "-");

            if (YName != "")
                strCsv += String.Format(",{0} [{1}]", YName, YUnit != "" ? YUnit : "-");
            else
                strCsv += String.Format(",{0}", YUnit != "" ? YUnit : "-");

            try
            {
                System.IO.File.AppendAllText(fullFilePath, strCsv + "\r\n");
            }
            catch (Exception ex) { UnityEngine.Debug.Log(ex.Message); }

            for (int i = 0; i < _values.Length; i++)
            {
                strCsv = String.Format("{0}, {1:" + StringFormat.Replace("N","F") + "}", _values[i].x, _values[i].y);

                try
                {
                    System.IO.File.AppendAllText(fullFilePath, strCsv + "\r\n");
                }
                catch (Exception) { }
            }
        }
    }
}
