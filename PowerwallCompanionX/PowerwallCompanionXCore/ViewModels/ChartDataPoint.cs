using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerwallCompanionX.ViewModels
{
    public class ChartDataPoint
    {
        public ChartDataPoint(IComparable xValue, double yValue)
        {
            XValue = xValue;
            YValue = yValue;
        }

        public IComparable XValue { get; }
        public double YValue { get; }
    }
}
