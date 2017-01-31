using Finanse.DataAccessLayer;
using Finanse.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class StatisticsData {

        public ObservableCollection<ChartPart> setExpensesToIncomesChartValues(DateTime minDate, DateTime maxDate) {
            List<double> expenseToIncomeComparsion = StatisticsDal.getExpenseToIncomeComparsion(minDate, maxDate);
            ObservableCollection<ChartPart> expensesToIncomes = new ObservableCollection<ChartPart>();

            double sum = expenseToIncomeComparsion.Sum();

            expensesToIncomes.Add(new ChartPart {
                SolidColorBrush = (SolidColorBrush)Application.Current.Resources["Background+2"] as SolidColorBrush,
                Name = "Expense",
                RelativeValue = expenseToIncomeComparsion[0] / sum,
            });

            expensesToIncomes.Add(new ChartPart {
                SolidColorBrush = (SolidColorBrush)Application.Current.Resources["AccentColor"] as SolidColorBrush,
                Name = "Income",
                RelativeValue = expenseToIncomeComparsion[1] / sum,
            });

            return expensesToIncomes;
        }

        public ObservableCollection<ChartPart> setRelativeValues(List<ChartPart> list) {
            ObservableCollection<ChartPart> collection = new ObservableCollection<ChartPart>(list);
            double sum = collection.Sum(i => i.UnrelativeValue);

            foreach (ChartPart part in collection) {
                part.RelativeValue = part.UnrelativeValue / sum;

                string percentagePart = (part.RelativeValue >0.0001) ? 
                    (part.RelativeValue * 100).ToString("0.00") : "<0.01";
                part.Name += " (" + percentagePart + "%)";
            }

            return collection;
        }

        public string getActualDateRangeText(DateTime minDate, DateTime maxDate) {
            return minDate.ToString("d") + "   |   " + maxDate.ToString("d");
        }
    }
}
