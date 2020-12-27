using System;
using System.Collections.Generic;
using System.Linq;

namespace Graphing
{
    /// <summary>
    /// The class representing a graphing frame on-screen.
    /// </summary>
    public sealed class Grapher : GraphableCollection3, IDisposable
    {
        /// <summary>
        /// The actual graph texture.
        /// </summary>
        public UnityEngine.Texture2D graphTex;
        /// <summary>
        /// The horizontal axis texture.
        /// </summary>
        public UnityEngine.Texture2D hAxisTex;
        /// <summary>
        /// The vertical axis texture.
        /// </summary>
        public UnityEngine.Texture2D vAxisTex;
        /// <summary>
        /// The color axis texture.
        /// </summary>
        public UnityEngine.Texture2D cAxisTex;

        private AxesSettingWindow axesWindow;

        /// <summary>
        /// When true, reports the actual bounds of the contained objects rather than their self-reported bounds.
        /// </summary>
        public override bool AutoFitAxes
        {
            get => base.AutoFitAxes;
            set
            {
                if (value != AutoFitAxes)
                {
                    base.AutoFitAxes = value;
                    graphDirty = true;
                }
            }
        }

        /// <summary>
        /// The color axis lower bound.
        /// </summary>
        public float CMin { get; set; } = float.NaN;
        /// <summary>
        /// The color axis upper bound.
        /// </summary>
        public float CMax { get; set; } = float.NaN;

        /// <summary>
        /// The horizontal axis.
        /// </summary>
        public Axis horizontalAxis = new Axis(0, 0, true);
        /// <summary>
        /// The vertical axis.
        /// </summary>
        public Axis verticalAxis = new Axis(0, 0, false);
        /// <summary>
        /// The color axis.
        /// </summary>
        public Axis colorAxis = new Axis(0, 0);

        public delegate void AxesChangeEventHandler(object sender, AxesChangeEventArgs e);
        public event AxesChangeEventHandler AxesChanged;
        public delegate void AxesChangeRequestedEventHandler(object sender, AxesChangeRequestedEventArgs e);
        public event AxesChangeRequestedEventHandler AxesChangeRequested;
        internal delegate void ExternalValueChangeHandler(object sender, ExternalValueChangeEventArgs e);
        internal event ExternalValueChangeHandler ValueChangedExternally;

        internal float selfXmin, selfXmax, selfYmin, selfYmax, selfZmin, selfZmax;
        internal float setXmin, setXmax, setYmin, setYmax, setZmin, setZmax;
        internal bool[] useSelfAxes = new bool[] { true, true, true };

        bool axesDirty = true;
        bool graphDirty = true;

        /// <summary>
        /// Constructs new <see cref="Grapher"/> with the specified size for the graphing frame.
        /// </summary>
        /// <param name="width">The width of the graphing frame in pixels.</param>
        /// <param name="height">The height of the graphing frame in pixels.</param>
        /// <param name="axisWidth">The width of the axes textures in pixels.</param>
        public Grapher(int width, int height, int axisWidth)
        {
            this.graphTex = new UnityEngine.Texture2D(width, height, UnityEngine.TextureFormat.ARGB32, false);
            this.hAxisTex = new UnityEngine.Texture2D(width, axisWidth, UnityEngine.TextureFormat.ARGB32, false);
            this.vAxisTex = new UnityEngine.Texture2D(axisWidth, height, UnityEngine.TextureFormat.ARGB32, false);
            this.cAxisTex = new UnityEngine.Texture2D(width, axisWidth, UnityEngine.TextureFormat.ARGB32, false);

            setXmin = setXmax = setYmin = setYmax = setZmin = setZmax = float.NaN;

            axesWindow = new AxesSettingWindow(this);
        }
        /// <summary>
        /// Constructs new <see cref="Grapher"/> with the specified size for the graphing frame and with the specified contents.
        /// </summary>
        /// <param name="width">The width of the graphing frame in pixels.</param>
        /// <param name="height">The height of the graphing frame in pixels.</param>
        /// <param name="axisWidth">The width of the axes textures in pixels.</param>
        /// <param name="graphs">The <see cref="IGraphable"/>s to be included.</param>
        public Grapher(int width, int height, int axisWidth, IEnumerable<IGraphable> graphs) : this(width, height, axisWidth)
        {
            AddRange(graphs);
        }

        /// <summary>
        /// Spawns the <see cref="AxesSettingWindow"/>.
        /// </summary>
        /// <returns></returns>
        public PopupDialog SpawnAxesWindow() => axesWindow.SpawnPopupDialog();
        
        /// <summary>
        /// Force the specified axis to have certain bounds.
        /// </summary>
        /// <param name="axisIndex">0 = x, 1 = y, 2 = z</param>
        /// <param name="min">The lower bound.</param>
        /// <param name="max">The upper bound.</param>
        /// <param name="delayRecalculate">When true, <see cref="Grapher.RecalculateLimits"/> must be explicitly called later.</param>
        public void SetAxesLimits(int axisIndex, float min, float max, bool delayRecalculate = false)
        {
            if (axisIndex > 2 || axisIndex < 0)
                throw new ArgumentException("Axis index must be between 0 and 2, inclusive.", "axisIndex");
            if (float.IsNaN(min) || float.IsInfinity(min))
                throw new ArgumentException("Axis limit must be a real number.", "min");
            if (float.IsNaN(max) || float.IsInfinity(max))
                throw new ArgumentException("Axis limit must be a real number.", "max");

            float tempMin = min, tempMax = max;
            switch (axisIndex)
            {
                case 0:
                    if (min != setXmin || max != setXmax)
                        OnAxesChangeRequested(axisIndex, ref min, ref max);
                    setXmin = min;
                    setXmax = max;
                    break;
                case 1:
                    if (min != setYmin || max != setYmax)
                        OnAxesChangeRequested(axisIndex, ref min, ref max);
                    setYmin = min;
                    setYmax = max;
                    break;
                case 2:
                    if (min != setZmin || max != setZmax)
                        OnAxesChangeRequested(axisIndex, ref min, ref max);
                    setZmin = min;
                    setZmax = max;
                    break;
            }
            if (tempMin != min || tempMax != max)
                ValueChangedExternally?.Invoke(this, new ExternalValueChangeEventArgs(axisIndex, min, max));
            useSelfAxes[axisIndex] = false;

            if (!delayRecalculate)
                RecalculateLimits();
        }
        /// <summary>
        /// Resets the specified axis to self-determined bounds.
        /// </summary>
        /// <param name="index">-1 = all, 0 = x, 1 = y, 2 = z</param>
        /// <param name="delayRecalculate">When true, <see cref="Grapher.RecalculateLimits"/> must be explicitly called later.</param>
        public void ReleaseAxesLimits(int index = -1, bool delayRecalculate = false)
        {
            if (index >= 0)
                useSelfAxes[index] = true;
            else
            {
                ReleaseAxesLimits(0, true);
                ReleaseAxesLimits(1, true);
                ReleaseAxesLimits(2, true);
            }
            if (delayRecalculate)
                return;
            RecalculateLimits();
        }
        /// <summary>
        /// Resets the stored prescribed bounds for the selected axis.
        /// </summary>
        /// <param name="index">-1 = all, 0 = x, 1 = y, 2 = z</param>
        /// <param name="deferRecalculate">When true, <see cref="Grapher.RecalculateLimits"/> must be explicitly called later.</param>
        public void ResetStoredLimits(int index = -1, bool deferRecalculate = false)
        {
            switch (index)
            {
                case 0:
                    setXmin = setXmax = float.NaN;
                    break;
                case 1:
                    setYmin = setYmax = float.NaN;
                    break;
                case 2:
                    setZmin = setZmax = float.NaN;
                    break;
                case -1:
                    ResetStoredLimits(0, true);
                    ResetStoredLimits(1, true);
                    ResetStoredLimits(2, true);
                    break;
            }
            if (index >= 0 && !deferRecalculate && !useSelfAxes[index])
                RecalculateLimits();
        }

        public void OnAxesChanged()
            => this.AxesChanged?.Invoke(this, new AxesChangeEventArgs(XMin, XMax, YMin, YMax, ZMin, ZMax));
        public void OnAxesChangeRequested(int index, ref float min, ref float max)
            => this.AxesChangeRequested?.Invoke(this, new AxesChangeRequestedEventArgs(index, min, max));

        /// <summary>
        /// Recalculates the reported limits of the <see cref="GraphableCollection"/>.
        /// </summary>
        /// <returns></returns>
        public override bool RecalculateLimits()
        {
            float[] oldLimits = new float[] { XMin, XMax, YMin, YMax, ZMin, ZMax, CMin, CMax };

            bool baseResult = base.RecalculateLimits();

            CMin = CMax = float.NaN;
            for (int i = 0; i < graphs.Count; i++)
            {
                if (!graphs[i].Visible) continue;
                if (graphs[i] is SurfGraph surfGraph)
                {
                    if (float.IsNaN(CMin) || CMin > surfGraph.CMin) CMin = surfGraph.CMin;
                    if (float.IsNaN(CMax) || CMax < surfGraph.CMax) CMax = surfGraph.CMax;
                }
            }
            // If color is somehow not set...
            if (float.IsNaN(CMin)) CMin = ZMin;
            if (float.IsNaN(CMax)) CMax = ZMax;
            // Check that the ColorMap will accept our limits, otherwise attempt to find new ones.
            if (!float.IsNaN(CMin) && !dominantColorMap.Filter(CMin))
            {
                if (float.IsNegativeInfinity(CMin) && dominantColorMap.Filter(float.MinValue)) CMin = float.MinValue;
                else if (CMin < 0 && dominantColorMap.Filter(0)) CMin = 0;
            }
            if (!float.IsNaN(CMax) && !dominantColorMap.Filter(CMax))
            {
                if (float.IsPositiveInfinity(CMax) && dominantColorMap.Filter(float.MaxValue)) CMax = float.MaxValue;
                else if (CMax > 0 && dominantColorMap.Filter(0)) CMax = 0;
            }

            bool setsNaN = float.IsNaN(setXmin) || float.IsNaN(setXmax) || float.IsNaN(setYmin) || float.IsNaN(setYmax) || float.IsNaN(setZmin) || float.IsNaN(setZmax);
            if (((useSelfAxes[0] || useSelfAxes[1] || useSelfAxes[2]) && baseResult) || setsNaN)
            {
                horizontalAxis = new Axis(XMin, XMax, true);
                verticalAxis = new Axis(YMin, YMax, false);
                colorAxis = new Axis(CMin, CMax);

                XMin = selfXmin = horizontalAxis.Min;
                XMax = selfXmax = horizontalAxis.Max;
                YMin = selfYmin = verticalAxis.Min;
                YMax = selfYmax = verticalAxis.Max;
                ZMin = CMin = selfZmin = colorAxis.Min;
                ZMax = CMax = selfZmax = colorAxis.Max;

                if (setsNaN)
                {
                    if (float.IsNaN(setXmin)) setXmin = selfXmin;
                    if (float.IsNaN(setXmax)) setXmax = selfXmax;
                    if (float.IsNaN(setYmin)) setYmin = selfYmin;
                    if (float.IsNaN(setYmax)) setYmax = selfYmax;
                    if (float.IsNaN(setZmin)) setZmin = selfZmin;
                    if (float.IsNaN(setZmax)) setZmax = selfZmax;
                }
            }
            if (!useSelfAxes[0])
            {
                XMin = setXmin; XMax = setXmax;
                horizontalAxis = new Axis(XMin, XMax, true, true);
            }
            if (!useSelfAxes[1])
            {
                YMin = setYmin; YMax = setYmax;
                verticalAxis = new Axis(YMin, YMax, false, true);
            }
            if (!useSelfAxes[2])
            {
                ZMin = CMin = setZmin; ZMax = CMax = setZmax;
                colorAxis = new Axis(CMin, CMax, forceBounds: true);
            }

            bool boundAxesChanged = !(oldLimits[0] == XMin && oldLimits[1] == XMax && oldLimits[2] == YMin && oldLimits[3] == YMax);
            if (boundAxesChanged)
                OnAxesChanged();

            if (axesDirty || boundAxesChanged || !(oldLimits[4] == ZMin && oldLimits[5] == ZMax && oldLimits[6] == CMin && oldLimits[7] == CMax))
            {
                graphDirty = true;
                axesDirty = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets a <see cref="UnityEngine.Texture2D"/> to <see cref="UnityEngine.Color.clear"/>.
        /// </summary>
        /// <param name="texture"></param>
        public static void ClearTexture(ref UnityEngine.Texture2D texture)
        {
            ClearTexture(ref texture, UnityEngine.Color.clear);
        }
        /// <summary>
        /// Resets a <see cref="UnityEngine.Texture2D"/> to a specified color.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="color"></param>
        public static void ClearTexture(ref UnityEngine.Texture2D texture, UnityEngine.Color color)
        {
            UnityEngine.Color[] pixels = texture.GetPixels();
            for (int i = pixels.Length - 1; i >= 0; i--)
                pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
        }

        /// <summary>
        /// Draws the contained graphs on <see cref="graphTex"/>.
        /// </summary>
        public void DrawGraphs()
        {
            if (!graphDirty)
                return;

            RecalculateLimits();
            if (axesDirty)
            {
                ClearTexture(ref hAxisTex);
                ClearTexture(ref vAxisTex);
                //ClearTexture(ref cAxisTex);
                horizontalAxis.DrawAxis(ref hAxisTex, UnityEngine.Color.white);
                verticalAxis.DrawAxis(ref vAxisTex, UnityEngine.Color.white);
                DrawColorAxis(ref cAxisTex, dominantColorMap);
                colorAxis.DrawAxis(ref cAxisTex, UnityEngine.Color.white, false);
                axesDirty = false;
            }
            ClearTexture(ref graphTex);
            
            this.Draw(ref this.graphTex, XMin, XMax, YMin, YMax, CMin, CMax);

            graphDirty = false;
        }

        /// <summary>
        /// Gets the value of the graph at the specified index at the specified pixel coordinates.
        /// </summary>
        /// <param name="x">The x pixel coordinate.</param>
        /// <param name="y">The y pixel coordinate.</param>
        /// <param name="index">The index of the graph from which to report.</param>
        /// <returns></returns>
        public float ValueAtPixel(int x, int y, int index = 0)
        {
            if (graphs.Count - 1 < index)
                return float.NaN;

            float xVal = x / (float)(graphTex.width - 1) * (XMax - XMin) + XMin;
            float yVal = y / (float)(graphTex.height - 1) * (YMax - YMin) + YMin;

            return ValueAt(xVal, yVal, index);
        }

        /// <summary>
        /// Gets a formatted value from the graphs at the specified pixel coordinates.
        /// </summary>
        /// <param name="x">The x pixel coordinate.</param>
        /// <param name="y">The y pixel coordinate.</param>
        /// <param name="index">The index of the graph from which to report, or -1 for al graphs.</param>
        /// <returns></returns>
        public string GetFormattedValueAtPixel(int xPix, int yPix, int index = -1)
        {
            if (graphs.Count == 0)
                return "";

            float xVal = xPix / (float)(graphTex.width - 1) * (XMax - XMin) + XMin;
            float yVal = yPix / (float)(graphTex.height - 1) * (YMax - YMin) + YMin;

            return GetFormattedValueAt(xVal, yVal, index, false);
        }

        /// <summary>
        /// Sets the <see cref="GraphableCollection"/> to contain the provided <see cref="IGraphable"/>s.
        /// </summary>
        /// <param name="newCollection"></param>
        public void SetCollection(IEnumerable<IGraphable> newCollection)
        {
            this.Graphables = newCollection.ToList();
            ResetStoredLimits();
        }

        /// <summary>
        /// Returns the primary graph frame texture.
        /// </summary>
        /// <param name="graph"></param>
        public static explicit operator UnityEngine.Texture2D(Grapher graph)
        {
            return graph.graphTex;
        }

        /// <summary>
        /// Draws the provided <see cref="ColorMap"/> as a texture.
        /// </summary>
        /// <param name="axisTex"></param>
        /// <param name="colorMap"></param>
        public static void DrawColorAxis(ref UnityEngine.Texture2D axisTex, ColorMap colorMap)
        {
            int width = axisTex.width - 1;
            int height = axisTex.height - 1;
            bool horizontal = height <= width;
            int major = horizontal ? width : height;
            int minor = horizontal ? height : width;

            for (int a = 0; a <= major; a++)
            {
                UnityEngine.Color rowColor = colorMap[(float)a / major];
                for (int b = 0; b <= minor; b++)
                {
                    if (horizontal)
                        axisTex.SetPixel(a, b, rowColor);
                    else
                        axisTex.SetPixel(b, a, rowColor);
                }
            }

            axisTex.Apply();
        }

        /// <summary>
        /// Subscribes to the contained objects' <see cref="IGraphable.ValuesChanged"/> events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ValuesChangedSubscriber(object sender, EventArgs e)
        {
            graphDirty = true;
            base.ValuesChangedSubscriber(sender, e);
        }
        /// <summary>
        /// Invokes the <see cref="ValuesChanged"/> event for this object.
        /// </summary>
        /// <param name="eventArgs">Any relevant <see cref="EventArgs"/>.</param>
        protected override void OnValuesChanged(EventArgs eventArgs)
        {
            graphDirty = true;
            base.OnValuesChanged(eventArgs);
        }

        /// <summary>
        /// Implements <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            UnityEngine.Object.Destroy(graphTex);
            UnityEngine.Object.Destroy(hAxisTex);
            UnityEngine.Object.Destroy(vAxisTex);
            UnityEngine.Object.Destroy(cAxisTex);
        }

        /// <summary>
        /// Removes all elements from the <see cref="GraphableCollection"/>.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            ResetStoredLimits();
        }
        /// <summary>
        /// Removes the first occurrence of the specified object.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public override bool Remove(IGraphable graph)
        {
            bool result = base.Remove(graph);
            if (result && this.Count == 0)
                ResetStoredLimits();
            return result;
        }
        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            if (this.Count == 0)
                ResetStoredLimits();
        }
    }
}
