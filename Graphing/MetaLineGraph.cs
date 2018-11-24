using System;
using System.Linq;

namespace Graphing
{
    /// <summary>
    /// A class representing a line graph with associated metadata.
    /// </summary>
    public class MetaLineGraph : LineGraph, ILineGraph
    {
        protected float[][] metaData;
        /// <summary>
        /// MetaData associated with each point on the <see cref="LineGraph"/>.
        /// </summary>
        public float[][] MetaData { get => metaData; set => SetMetaData(value); }
        /// <summary>
        /// The names of the <see cref="MetaData"/> fields.
        /// </summary>
        public string[] MetaFields { get; set; } = new string[0];
        /// <summary>
        /// Format strings for outputting the <see cref="MetaData"/>.
        /// </summary>
        public string[] MetaStringFormats { get; set; } = new string[0];
        /// <summary>
        /// Units for the <see cref="MetaData"/>.
        /// </summary>
        public string[] MetaUnits { get; set; } = new string[0];
        protected int metaCount = 0;
        /// <summary>
        /// Gets the number of <see cref="MetaData"/> fields.
        /// </summary>
        public int MetaFieldCount { get => metaCount; }


        /// <summary>
        /// Constructs a new <see cref="MetaLineGraph"/> with the provided y-values at evenly spaced points.
        /// </summary>
        /// <param name="xLeft">The x-value at the leftmost point.</param>
        /// <param name="xRight">The x-value at the rightmost point.</param>
        /// <param name="values"></param>
        public MetaLineGraph(float[] values, float xLeft, float xRight) : base(values, xLeft, xRight)
        {
            metaData = new float[0][];
            MetaFields = new string[0];
        }
        /// <summary>
        /// Constructs a new <see cref="MetaLineGraph"/> with the provided points.
        /// </summary>
        /// <param name="values"></param>
        public MetaLineGraph(UnityEngine.Vector2[] values) : base(values)
        {
            metaData = new float[0][];
            MetaFields = new string[0];
        }

        /// <summary>
        /// Constructs a new <see cref="MetaLineGraph"/> with the provided y-values at evenly spaced points.
        /// </summary>
        /// <param name="xLeft">The x-value at the leftmost point.</param>
        /// <param name="xRight">The x-value at the rightmost point.</param>
        /// <param name="values"></param>
        public MetaLineGraph(float[] values, float xLeft, float xRight, string[] metaFields, float[][] metaData)
            : base(values, xLeft, xRight)
        {
            metaCount = metaData.Length;
            this.metaData = metaData;
            this.MetaFields = new string[metaCount];
            metaFields.CopyTo(this.MetaFields, 0);
        }
        /// <summary>
        /// Constructs a new <see cref="MetaLineGraph"/> with the provided points.
        /// </summary>
        /// <param name="values"></param>
        public MetaLineGraph(UnityEngine.Vector2[] values, string[] metaFields, float[][] metaData)
            : base(values)
        {
            metaCount = metaData.Length;
            this.metaData = metaData;
            this.MetaFields = new string[metaCount];
            metaFields.CopyTo(this.MetaFields, 0);
        }
        
        /// <summary>
        /// Set the associated MetaData.
        /// </summary>
        /// <param name="metaData"></param>
        public void SetMetaData(float[][] metaData) => SetMetaData(metaData, Values.Length);
        private void SetMetaData(float[][] metaData, int length)
        {
            for (int i = 0; i < metaData.Length; i++)
            {
                if (metaData[i].Length != length)
                    throw new ArgumentOutOfRangeException("metaData");
            }
            AdjustArrayLengths(metaData.Length);
            this.metaData = metaData;
        }
        /// <summary>
        /// Sets the values in the line with the provided y-values at evenly spaced points.
        /// </summary>
        /// <param name="xLeft">The x-value at the leftmost point.</param>
        /// <param name="xRight">The x-value at the rightmost point.</param>
        /// <param name="values"></param>
        public void SetValues(float[] values, float xLeft, float xRight, float[][] metaData)
        {
            SetMetaData(metaData, values.Length);
            SetValues(values, xLeft, xRight);
        }
        /// <summary>
        /// Sets the values in the line.
        /// </summary>
        /// <param name="values"></param>
        public void SetValues(UnityEngine.Vector2[] values, float[][] metaData)
        {
            SetMetaData(metaData, values.Length);
            SetValues(values);
        }

        private void AdjustArrayLengths(int length)
        {
            if (length > metaCount)
            {
                metaCount = length;
                string[] fields = new string[metaCount];
                MetaFields.CopyTo(fields, 0);
                MetaFields = fields;
                string[] formats = new string[metaCount];
                MetaStringFormats.CopyTo(formats, 0);
                MetaStringFormats = formats;
                string[] units = new string[metaCount];
                MetaUnits.CopyTo(units, 0);
                MetaUnits = units;
            }
            else
                metaCount = length;
        }

        /// <summary>
        /// Gets the metadata given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <returns></returns>
        public float MetaValueAt(float x, float y, int metaIndex)
            => MetaValueAt(x, y, 1, 1, metaIndex);
        /// <summary>
        /// Gets the metadata given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="width">The x domain of the graph.</param>
        /// <param name="height">The y range of the graph.</param>
        /// <returns></returns>
        public float MetaValueAt(float x, float y, float width, float height, int metaIndex)
        {
            if (metaIndex > metaCount)
                return 0;
            if (metaData[metaIndex].Length <= 0)
                return 0;

            if (Transpose) x = y;

            if (equalSteps && sorted)
            {
                if (x <= XMin) return metaData[metaIndex][0];

                int length = _values.Length - 1;
                if (x >= XMax) return metaData[metaIndex][length];

                float step = (XMax - XMin) / length;
                int index = (int)Math.Floor((x - XMin) / step);
                float f = (x - XMin) / step % 1;
                if (f == 0)
                    return metaData[metaIndex][index];
                return metaData[metaIndex][index] * (1 - f) + metaData[metaIndex][index + 1] * f;
            }
            else
            {
                if (sorted)
                {
                    //if (x <= Values[0].x)
                    //    return Values[0].y;
                    if (x >= _values[_values.Length - 1].x)
                        return metaData[metaIndex][_values.Length - 1];
                    for (int i = _values.Length - 2; i >= 0; i--)
                    {
                        if (x > _values[i].x)
                        {
                            float f = (x - _values[i].x) / (_values[i + 1].x - _values[i].x);
                            return metaData[metaIndex][i] * (1 - f) + metaData[metaIndex][i + 1] * f;
                        }
                    }
                    return metaData[metaIndex][0];
                }
                else
                {
                    UnityEngine.Vector2 point = new UnityEngine.Vector2(x, y);
                    UnityEngine.Vector2 closestPoint = _values[0];
                    float result = metaData[metaIndex][0];
                    float currentDistance = float.PositiveInfinity;
                    int length = _values.Length;
                    for (int i = 0; i < length - 1 - 1; i++)
                    {
                        float lineLength = (_values[i + 1] - _values[i]).magnitude;
                        UnityEngine.Vector2 lineDir = (_values[i + 1] - _values[i]) / lineLength;
                        float distance = UnityEngine.Vector2.Dot(point - _values[i], lineDir);
                        UnityEngine.Vector2 closestPt = _values[i] + distance * lineDir;
                        if (distance <= 0)
                        {
                            closestPt = _values[i];
                            distance = 0;
                        }
                        else if ((closestPt - _values[i]).sqrMagnitude > (_values[i + 1] - _values[i]).sqrMagnitude)//(distance * distance >= (_values[i + 1] - _values[i]).sqrMagnitude)
                        {
                            closestPt = _values[i + 1];
                            distance = lineLength;
                        }
                        UnityEngine.Vector2 LocalTransform(UnityEngine.Vector2 vector) => new UnityEngine.Vector2(vector.x / width, vector.y / height);
                        float ptDistance = LocalTransform(point - closestPt).sqrMagnitude;

                        if ( ptDistance < currentDistance)
                        {
                            currentDistance = ptDistance;
                            closestPoint = closestPt;
                            distance = distance / lineLength;
                            result = metaData[metaIndex][i] * (1 - distance) + metaData[metaIndex][i + 1] * distance;
                        }
                    }
                    return result;
                }
            }
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
        public override string GetFormattedValueAt(float x, float y, float width, float height, bool withName = false)
        {
            if (!Visible || Values.Length <= 0) return "";
            string value = base.GetFormattedValueAt(x, y, width, height, withName);
            for (int i = 0; i < metaCount; i++)
            {
                if (MetaStringFormats.Length > i)
                    value += String.Format("\n{2}{0:" + (String.IsNullOrEmpty(MetaStringFormats[i]) ? "" : MetaStringFormats[i]) + "}{1}", MetaValueAt(x, y, width, height, i), MetaUnits.Length <= i || String.IsNullOrEmpty(MetaUnits[i]) ? "" : MetaUnits[i], String.IsNullOrEmpty(MetaFields[i]) ? "" : MetaFields[i] + ": ");
                else
                    value += String.Format("\n{2}{0}{1}", MetaValueAt(x, y, width, height, i).ToString(), MetaUnits.Length <= i || String.IsNullOrEmpty(MetaUnits[i]) ? "" : MetaUnits[i], String.IsNullOrEmpty(MetaFields[i]) ? "" : MetaFields[i] + ": ");
            }
            return value;
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

            for (int i = 0; i < MetaFields.Length && i < metaCount; i++)
            {
                string nameStr = String.IsNullOrEmpty(MetaFields[i]) ? "" : MetaFields[i];
                string unitStr = MetaUnits.Length <= i || String.IsNullOrEmpty(MetaUnits[i]) ? "-" : MetaUnits[i];
                strCsv += nameStr != "" ? String.Format(",{0} [{1}]", nameStr, unitStr) : String.Format(",{0}", unitStr);
            }

            try
            {
                System.IO.File.AppendAllText(fullFilePath, strCsv + "\r\n");
            }
            catch (Exception ex) { UnityEngine.Debug.Log(ex.Message); }

            for (int i = 0; i < _values.Length; i++)
            {
                strCsv = String.Format("{0}, {1:" + StringFormat.Replace("N", "F") + "}", _values[i].x, _values[i].y);
                for (int j = 0; j < metaCount; j++)
                {
                    if (MetaStringFormats.Length >= j)
                        strCsv += "," + metaData[j][i].ToString(MetaStringFormats[j].Replace("N", "F"));
                    else
                        strCsv += "," + metaData[j][i].ToString();
                }

                try
                {
                    System.IO.File.AppendAllText(fullFilePath, strCsv + "\r\n");
                }
                catch (Exception) { }
            }
        }
    }
}
