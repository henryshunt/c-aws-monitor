using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;

namespace C_AWSMonitor.Routines
{
    public class ChartViewModel
    {
        public List<DataPoint> Points { get; private set; }
            = new List<DataPoint>();

        public void AddPoint(DateTime time, double value)
        {
            Points.Add(new DataPoint(
                DateTimeAxis.ToDouble(time), value));
        }

        public void ClearPoints()
        {
            Points.Clear();
        }

        public double GetMinimum()
        {
            double? minimum = null;
            foreach (DataPoint point in Points)
            {
                if (minimum == null) minimum = point.Y;
                else
                {
                    if (point.Y < minimum) minimum = point.Y;
                }
            }

            return minimum == null ? 0 : (double)minimum;
        }
        public double GetMaximum()
        {
            double? maximum = null;
            foreach (DataPoint point in Points)
            {
                if (maximum == null) maximum = point.Y;
                else
                {
                    if (point.Y > maximum) maximum = point.Y;
                }
            }

            return maximum == null ? 0 : (double)maximum;
        }
    }
}
