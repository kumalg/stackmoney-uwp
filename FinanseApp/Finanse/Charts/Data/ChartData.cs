using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Finanse.Charts.Data
{
    public class ChartData : List<ChartDataItem>
    {
        public static ChartData PrepareChartData(IEnumerable<byte> source, Func<byte, SolidColorBrush> colorProp = null, Func<byte, string> nameProp = null, Func<byte, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<sbyte> source, Func<sbyte, SolidColorBrush> colorProp = null, Func<sbyte, string> nameProp = null, Func<sbyte, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<short> source, Func<short, SolidColorBrush> colorProp = null, Func<short, string> nameProp = null, Func<short, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<ushort> source, Func<ushort, SolidColorBrush> colorProp = null, Func<ushort, string> nameProp = null, Func<ushort, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<int> source, Func<int, SolidColorBrush> colorProp = null, Func<int, string> nameProp = null, Func<int, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<uint> source, Func<uint, SolidColorBrush> colorProp = null, Func<uint, string> nameProp = null, Func<uint, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<long> source, Func<long, SolidColorBrush> colorProp = null, Func<long, string> nameProp = null, Func<long, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<ulong> source, Func<ulong, SolidColorBrush> colorProp = null, Func<ulong, string> nameProp = null, Func<ulong, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<decimal> source, Func<decimal, SolidColorBrush> colorProp = null, Func<decimal, string> nameProp = null, Func<decimal, string> unitProp = null)
            => PrepareChartData(source, d => (double)d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<float> source, Func<float, SolidColorBrush> colorProp = null, Func<float, string> nameProp = null, Func<float, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);
        public static ChartData PrepareChartData(IEnumerable<double> source, Func<double, SolidColorBrush> colorProp = null, Func<double, string> nameProp = null, Func<double, string> unitProp = null)
            => PrepareChartData(source, d => d, colorProp, nameProp, unitProp);

        public static ChartData PrepareChartData<T>(IEnumerable<T> source, Func<T, double> valueProp, Func<T, SolidColorBrush> colorProp = null, Func<T, string> nameProp = null, Func<T, string> unitProp = null)
        {
            var data = new ChartData();
            var values = source.Select(valueProp.Invoke).ToList();
            var totalValue = values.Sum();

            var sourceList = source.ToList();

            data.AddRange(
                sourceList.Select(
                    (t, i) => new ChartDataItem(
                        values[i] / totalValue,
                        colorProp?.Invoke(t) ?? new SolidColorBrush(default(Color)),
                        nameProp?.Invoke(t) ?? i.ToString(),
                        values[i],
                        unitProp?.Invoke(t)
                        )));

            return data;
        }
    }
}
