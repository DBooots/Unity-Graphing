using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphing
{
    public sealed class AxesChangeEventArgs : EventArgs
    {
        private readonly float xMin;
        private readonly float xMax;
        private readonly float yMin;
        private readonly float yMax;
        private readonly float zMin = float.NaN;
        private readonly float zMax = float.NaN;

        public float XMin => xMin;
        public float XMax => xMax;
        public float YMin => yMin;
        public float YMax => yMax;
        public float ZMin => zMin;
        public float ZMax => zMax;

        public AxesChangeEventArgs(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax) :
            this(xMin, xMax, yMin, yMax)
        {
            this.zMin = zMin;
            this.zMax = zMax;
        }
        public AxesChangeEventArgs(float xMin, float xMax, float yMin, float yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }
    }

    public sealed class AxesChangeRequestedEventArgs : EventArgs
    {
        private readonly int index;
        private float min;
        private float max;

        public char Axis { get => index == 0 ? 'x' : index == 1 ? 'y' : 'z'; }
        public int AxisIndex => index;
        public float Min { get => min; set => min = value; }
        public float Max { get => max; set => max = value; }

        public AxesChangeRequestedEventArgs(int axisIndex, float min, float max)
        {
            if (axisIndex > 2 || axisIndex < 0)
                throw new ArgumentException("Axis index must be between 0 and 2, inclusive.", "axisIndex");
            if (float.IsNaN(min) || float.IsInfinity(min))
                throw new ArgumentException("Axis limit must be a real number.", "min");
            if (float.IsNaN(max) || float.IsInfinity(max))
                throw new ArgumentException("Axis limit must be a real number.", "max");
            this.index = axisIndex;
            this.min = min;
            this.max = max;
        }
        public AxesChangeRequestedEventArgs(char axis, float min, float max)
        {
            switch (axis)
            {
                case 'X':
                case 'x':
                    index = 0;
                    break;
                case 'Y':
                case 'y':
                    index = 1;
                    break;
                case 'Z':
                case 'z':
                    index = 2;
                    break;
                default:
                    throw new ArgumentException("Axis index must be 'x', 'y', or 'z'.", "axis");
            }

            if (float.IsNaN(min) || float.IsInfinity(min))
                throw new ArgumentException("Axis limit must be a real number.", "min");
            if (float.IsNaN(max) || float.IsInfinity(max))
                throw new ArgumentException("Axis limit must be a real number.", "max");
            this.min = min;
            this.max = max;
        }
    }

    public sealed class ExternalValueChangeEventArgs : EventArgs
    {
        private readonly int index;
        private readonly float min;
        private readonly float max;

        public char Axis { get => index == 0 ? 'x' : index == 1 ? 'y' : 'z'; }
        public int AxisIndex => index;
        public float Min => min;
        public float Max => max;

        public ExternalValueChangeEventArgs(int axisIndex, float min, float max)
        {
            if (axisIndex > 2 || axisIndex < 0)
                throw new ArgumentException("Axis index must be between 0 and 2, inclusive.", "axisIndex");
            if (float.IsNaN(min) || float.IsInfinity(min))
                throw new ArgumentException("Axis limit must be a real number.", "min");
            if (float.IsNaN(max) || float.IsInfinity(max))
                throw new ArgumentException("Axis limit must be a real number.", "max");
            this.index = axisIndex;
            this.min = min;
            this.max = max;
        }
        public ExternalValueChangeEventArgs(char axis, float min, float max)
        {
            switch (axis)
            {
                case 'X':
                case 'x':
                    index = 0;
                    break;
                case 'Y':
                case 'y':
                    index = 1;
                    break;
                case 'Z':
                case 'z':
                    index = 2;
                    break;
                default:
                    throw new ArgumentException("Axis index must be 'x', 'y', or 'z'.", "axis");
            }

            if (float.IsNaN(min) || float.IsInfinity(min))
                throw new ArgumentException("Axis limit must be a real number.", "min");
            if (float.IsNaN(max) || float.IsInfinity(max))
                throw new ArgumentException("Axis limit must be a real number.", "max");
            this.min = min;
            this.max = max;
        }
    }
}
