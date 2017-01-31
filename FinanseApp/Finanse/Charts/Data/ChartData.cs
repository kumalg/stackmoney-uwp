using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace Finanse.Charts.Data
{
    public class ChartData : List<ChartDataItem>
    {
        public static ChartData PrepareChartData(IEnumerable<byte> source, Func<byte, Color> colorProp = null, Func<byte, string> nameProp = null, Func<byte, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<sbyte> source, Func<sbyte, Color> colorProp = null, Func<sbyte, string> nameProp = null, Func<sbyte, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<short> source, Func<short, Color> colorProp = null, Func<short, string> nameProp = null, Func<short, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<ushort> source, Func<ushort, Color> colorProp = null, Func<ushort, string> nameProp = null, Func<ushort, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<int> source, Func<int, Color> colorProp = null, Func<int, string> nameProp = null, Func<int, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<uint> source, Func<uint, Color> colorProp = null, Func<uint, string> nameProp = null, Func<uint, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<long> source, Func<long, Color> colorProp = null, Func<long, string> nameProp = null, Func<long, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<ulong> source, Func<ulong, Color> colorProp = null, Func<ulong, string> nameProp = null, Func<ulong, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<decimal> source, Func<decimal, Color> colorProp = null, Func<decimal, string> nameProp = null, Func<decimal, string> unitProp = null)
            => PrepareChartData(source, d => (double)d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<float> source, Func<float, Color> colorProp = null, Func<float, string> nameProp = null, Func<float, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<double> source, Func<double, Color> colorProp = null, Func<double, string> nameProp = null, Func<double, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);

        public static ChartData PrepareChartData<T>(IEnumerable<T> source, Func<T, double> valueProp, Func<T, Color> colorProp = null, Func<T, string> nameProp = null, Func<T, string> unitProp = null)
        {
            var data = new ChartData();
            var values = source.Select(valueProp.Invoke).ToList();
            var totalValue = values.Sum();

            var sourceList = source.ToList();
            for (var i = 0; i < sourceList.Count; i++)
                data.Add(new ChartDataItem(
                    values[i] / totalValue,
                    colorProp?.Invoke(sourceList[i]) ?? default(Color),
                    nameProp?.Invoke(sourceList[i]) ?? i.ToString(),
                    values[i],
                    unitProp?.Invoke(sourceList[i])
                    ));
            return data;
        }
    }
}
