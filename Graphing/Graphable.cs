using System;

namespace Graphing
{
    /// <summary>
    /// Provides an interface for any object that can be drawn on a graph.
    /// </summary>
    public interface IGraphable
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The display name of the object. Can be different than the <see cref="Name"/> of the object.
        /// </summary>
        string DisplayName { get; set; }
        /// <summary>
        /// The visibility status of the object.
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Should the value be displayed on mouseover.
        /// </summary>
        bool DisplayValue { get; set; }
        /// <summary>
        /// The lower X bound of the object.
        /// </summary>
        float XMin { get; set; }
        /// <summary>
        /// The upper X bound of the object.
        /// </summary>
        float XMax { get; set; }
        /// <summary>
        /// The lower Y bound of the object.
        /// </summary>
        float YMin { get; set; }
        /// <summary>
        /// The upper Y bound of the object.
        /// </summary>
        float YMax { get; set; }
        /// <summary>
        /// The unit for the X axis.
        /// </summary>
        string XUnit { get; set; }
        /// <summary>
        /// The unit for the Y axis.
        /// </summary>
        string YUnit { get; set; }
        /// <summary>
        /// The name of the X axis.
        /// </summary>
        string XName { get; set; }
        /// <summary>
        /// The name of the Y axis.
        /// </summary>
        string YName { get; set; }
        /// <summary>
        /// A function for adjusting X values.
        /// </summary>
        Func<float, float> XAxisScale { get; set; }
        /// <summary>
        /// A function for adjusting Y values.
        /// </summary>
        Func<float, float> YAxisScale { get; set; }
        /// <summary>
        /// Draws the object on the specified <see cref="UnityEngine.Texture2D"/>.
        /// </summary>
        /// <param name="texture">The texture on which to draw the object.</param>
        /// <param name="xLeft">The X axis lower bound.</param>
        /// <param name="xRight">The X axis upper bound.</param>
        /// <param name="yBottom">The Y axis lower bound.</param>
        /// <param name="yTop">The Y axis upper bound.</param>
        void Draw(ref UnityEngine.Texture2D texture, float xLeft, float xRight, float yBottom, float yTop);
        /// <summary>
        /// Gets a value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <returns></returns>
        float ValueAt(float x, float y);
        /// <summary>
        /// Gets a formatted value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="withName">When true, requests the object include its name.</param>
        /// <returns></returns>
        string GetFormattedValueAt(float x, float y, bool withName = false);
        /// <summary>
        /// An event to be triggered when an object's values change.
        /// </summary>
        event EventHandler ValuesChanged;
        /// <summary>
        /// Outputs the object's values to file.
        /// </summary>
        /// <param name="directory">The directory in which to place the file.</param>
        /// <param name="filename">The filename for the file.</param>
        /// <param name="sheetName">An optional sheet name for within the file.</param>
        void WriteToFile(string directory, string filename, string sheetName = "");
    }

    /// <summary>
    /// Provides an interface for any object that can be drawn on a graph and that has a Z component.
    /// </summary>
    public interface IGraphable3 : IGraphable
    {
        /// <summary>
        /// The lower Z bound of the object.
        /// </summary>
        float ZMin { get; set; }
        /// <summary>
        /// The upper Z bound of the object.
        /// </summary>
        float ZMax { get; set; }
        /// <summary>
        /// The unit for the Z axis.
        /// </summary>
        string ZUnit { get; set; }
        /// <summary>
        /// The name of the Z axis.
        /// </summary>
        string ZName { get; set; }
    }

    /// <summary>
    /// Provides an interface for an object that is specifically a graph.
    /// </summary>
    public interface IGraph : IGraphable
    {
        /// <summary>
        /// Defines the <see cref="ColorMap"/> for the graph.
        /// </summary>
        ColorMap Color { get; set; }
        /// <summary>
        /// Provides the mapping function as input to the <see cref="Color"/>.
        /// </summary>
        Graphable.CoordsToColorFunc ColorFunc { get; set; }
    }

    /// <summary>
    /// Provides an interface for an object that is specifically a line-based graph.
    /// </summary>
    public interface ILineGraph : IGraph
    {
        /// <summary>
        /// The width of the line in pixels.
        /// </summary>
        int LineWidth { get; set; }
        /// <summary>
        /// Gets a value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="width">The x domain of the graph.</param>
        /// <param name="height">The y range of the graph.</param>
        /// <returns></returns>
        float ValueAt(float x, float y, float width, float height);
        /// <summary>
        /// Gets a formatted value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="width">The x domain of the graph.</param>
        /// <param name="height">The y range of the graph.</param>
        /// <param name="withName">When true, requests the object include its name.</param>
        /// <returns></returns>
        string GetFormattedValueAt(float x, float y, float width, float height, bool withName = false);
    }

    /// <summary>
    /// An abstract class for directly graphable objects.
    /// </summary>
    public abstract class Graphable : IGraphable, IGraph
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        public string Name { get; set; } = "";

        private string displayName = "";
        /// <summary>
        /// The display name of the object. Can be different than the <see cref="Name"/> of the object.
        /// </summary>
        public string DisplayName
        {
            get => String.IsNullOrEmpty(displayName) ? Name : displayName;
            set => displayName = value;
        }
        /// <summary>
        /// The visibility status of the object.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                bool changed = _visible != value;
                _visible = value;
                if (changed) OnValuesChanged(null);
            }
        }
        private bool _visible = true;
        /// <summary>
        /// Should the value be displayed on mouseover.
        /// </summary>
        public bool DisplayValue { get; set; } = true;
        /// <summary>
        /// The lower X bound of the object.
        /// </summary>
        public virtual float XMin { get => xMin; set => xMin = value; }
        protected float xMin;
        /// <summary>
        /// The upper X bound of the object.
        /// </summary>
        public virtual float XMax { get => xMax; set => xMax = value; }
        protected float xMax;
        /// <summary>
        /// The lower Y bound of the object.
        /// </summary>
        public virtual float YMin { get => yMin; set => yMin = value; }
        protected float yMin;
        /// <summary>
        /// The upper Y bound of the object.
        /// </summary>
        public virtual float YMax { get => yMax; set => yMax = value; }
        protected float yMax;
        /// <summary>
        /// A flag indicating if this object should be transposed before drawing.
        /// </summary>
        public bool Transpose { get; set; } = false;
        /// <summary>
        /// The name of the X axis.
        /// </summary>
        public string XName { get; set; } = "";
        protected internal string yName = null;
        /// <summary>
        /// The name of the Y axis.
        /// </summary>
        public virtual string YName { get => String.IsNullOrEmpty(yName) ? DisplayName : yName; set => yName = value; }
        /// <summary>
        /// The unit for the X axis.
        /// </summary>
        public string XUnit { get; set; } = "";
        /// <summary>
        /// The unit for the Y axis.
        /// </summary>
        public string YUnit { get; set; } = "";
        /// <summary>
        /// A standard Format String for use in <see cref="float.ToString(string)"/>.
        /// </summary>
        public string StringFormat { get; set; } = "G";
        /// <summary>
        /// Defines the <see cref="ColorMap"/> for the graph.
        /// </summary>
        public virtual ColorMap Color { get; set; } = new ColorMap(UnityEngine.Color.white);
        /// <summary>
        /// Provides the mapping function as input to the <see cref="Color"/>.
        /// </summary>
        public delegate float CoordsToColorFunc(float x, float y, float z);
        /// <summary>
        /// Provides the mapping function as input to the <see cref="Color"/>.
        /// </summary>
        public virtual CoordsToColorFunc ColorFunc { get; set; } = (x, y, z) => 0;
        /// <summary>
        /// A function for adjusting X values.
        /// </summary>
        public virtual Func<float, float> XAxisScale { get; set; } = (v) => v;
        /// <summary>
        /// A function for adjusting Y values.
        /// </summary>
        public virtual Func<float, float> YAxisScale { get; set; } = (v) => v;

        /// <summary>
        /// Draws the object on the specified <see cref="UnityEngine.Texture2D"/>.
        /// </summary>
        /// <param name="texture">The texture on which to draw the object.</param>
        /// <param name="xLeft">The X axis lower bound.</param>
        /// <param name="xRight">The X axis upper bound.</param>
        /// <param name="yBottom">The Y axis lower bound.</param>
        /// <param name="yTop">The Y axis upper bound.</param>
        public abstract void Draw(ref UnityEngine.Texture2D texture, float xLeft, float xRight, float yBottom, float yTop);
        /// <summary>
        /// Gets a value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <returns></returns>
        public abstract float ValueAt(float x, float y);
        /// <summary>
        /// An event to be triggered when an object's values change.
        /// </summary>
        public event EventHandler ValuesChanged;

        /// <summary>
        /// Outputs the object's values to file.
        /// </summary>
        /// <param name="directory">The directory in which to place the file.</param>
        /// <param name="filename">The filename for the file.</param>
        /// <param name="sheetName">An optional sheet name for within the file.</param>
        public abstract void WriteToFile(string directory, string filename, string sheetName = "");

        /// <summary>
        /// Invokes the <see cref="ValuesChanged"/> event for this object.
        /// </summary>
        /// <param name="eventArgs">Any relevant <see cref="EventArgs"/>.</param>
        public virtual void OnValuesChanged(EventArgs eventArgs)
        {
            ValuesChanged?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Gets a formatted value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="withName">When true, requests the object include its name.</param>
        /// <returns></returns>
        public virtual string GetFormattedValueAt(float x, float y, bool withName = false)
        {
            return String.Format("{2}{0:" + StringFormat + "}{1}", ValueAt(x, y), YUnit, withName && !String.IsNullOrEmpty(DisplayName) ? DisplayName + ": " : "");
        }
    }

    /// <summary>
    /// An abstract class for directly graphable objects that have a Z component.
    /// </summary>
    public abstract class Graphable3 : Graphable, IGraphable3
    {
        /// <summary>
        /// The lower Z bound of the object.
        /// </summary>
        public float ZMin { get => zMin; set => zMin = value; }
        protected float zMin;
        /// <summary>
        /// The upper Z bound of the object.
        /// </summary>
        public float ZMax { get => zMax; set => zMax = value; }
        protected float zMax;
        /// <summary>
        /// The unit for the Z axis.
        /// </summary>
        public string ZUnit { get; set; }
        /// <summary>
        /// The name of the Y axis.
        /// </summary>
        public override string YName { get => yName ?? ""; set => yName = value; }
        protected string zName = null;
        /// <summary>
        /// The name of the Z axis.
        /// </summary>
        public string ZName { get { return String.IsNullOrEmpty(zName) ? DisplayName : zName; } set { zName = value; } }

        /// <summary>
        /// Gets a formatted value from the object given a selected coordinate.
        /// </summary>
        /// <param name="x">The x value of the selected coordinate.</param>
        /// <param name="y">The y value of the selected coordinate.</param>
        /// <param name="withName">When true, requests the object include its name.</param>
        /// <returns></returns>
        public override string GetFormattedValueAt(float x, float y, bool withName = false)
        {
            return String.Format("{2}{0:" + StringFormat + "}{1}", ValueAt(x, y), ZUnit, withName && !String.IsNullOrEmpty(DisplayName) ? DisplayName + ": " : "");
        }
    }
}
