using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;

namespace C_AWSMonitor
{
    public class ChartViewModel
    {
        public List<DataPoint> Points { get; private set; }
            = new List<DataPoint>();

        public ChartViewModel()
        {

        }

        public void AddPoint(long utc, double value)
        {
            Points.Add(new DataPoint(DateTimeAxis.ToDouble(EpochToUTC(utc)), value));
        }

        public void ClearPoints()
        {
            Points.Clear();
        }

        private DateTime EpochToUTC(long seconds)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(seconds);
        }

        public double GetMinimum()
        {
            double minimum = -99;
            foreach (DataPoint point in Points)
            {
                if (minimum == -99) { minimum = point.Y; }
                else
                {
                    if (point.Y < minimum) { minimum = point.Y; }
                }
            }

            return minimum == -99 ? 0 : minimum;
        }

        public double GetMaximum()
        {
            double maximum = -99;
            foreach (DataPoint point in Points)
            {
                if (maximum == -99) { maximum = point.Y; }
                else
                {
                    if (point.Y > maximum) { maximum = point.Y; }
                }
            }

            return maximum == -99 ? 0 : maximum;
        }
    }
}
