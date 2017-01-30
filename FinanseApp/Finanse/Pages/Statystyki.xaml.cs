using Finanse.DataAccessLayer;
using Finanse.Models;
using FourToolkit.Charts.Data;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace Finanse.Pages {

    public sealed partial class Statystyki : Page {
        
        DateTime minDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).Date;
        DateTime maxDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).Date;
        TextBlock dateRangeText = new TextBlock();
        TextBlock incomesPercentageText = new TextBlock();

        SortedDictionary<string, double> lista = new SortedDictionary<string, double>();
        Dictionary<string, double> expensesToIncomes = new Dictionary<string, double>();

        List<KeyValuePair<Color, double>> ExpensesToIncomes = new List<KeyValuePair<Color, double>>();

        private List<KeyValuePair<Color, double>> makeNewList(double expenses, double incomes) {
            double sum = expenses + incomes;
            double incomesPercentage = 100 * incomes / sum;
            Color expenseColor = ((SolidColorBrush)Application.Current.Resources["Background+2"] as SolidColorBrush).Color;
            Color incomeColor = ((SolidColorBrush)Application.Current.Resources["AccentColor"] as SolidColorBrush).Color;
            List<KeyValuePair<Color, double>> newList = new List<KeyValuePair<Color, double>>();
            newList.Add(new KeyValuePair<Color, double>(expenseColor, expenses/sum));
            newList.Add(new KeyValuePair<Color, double>(incomeColor, incomes/sum));

            incomesPercentageText.Text = incomesPercentage.ToString("#") + "%";

            return newList;
        }

        private void setActualDateRangeText() {
            dateRangeText.Text = minDate.ToString("d") + "   |   " + maxDate.ToString("d");
        }

        public Statystyki() {

            this.InitializeComponent();
            MinDatePicker.Date = minDate;
            MaxDatePicker.Date = maxDate;
            setActualDateRangeText();


            lista = StatisticsDal.getExpensesSummaryGroupedByCategoryInRange(minDate, maxDate);
            (ColumnChart.Series[0] as PieSeries).ItemsSource = lista;

            expensesToIncomes = StatisticsDal.getExpenseToIncomeComparsion(minDate, maxDate);

            ExpensesToIncomes = makeNewList(expensesToIncomes["expenses"], expensesToIncomes["incomes"]);
        }

        private void MinDatePicker_Closed(object sender, object e) {
            DateTime newMinDate = MinDatePicker.Date.Date;
            DateTime newMaxDate = MaxDatePicker.Date.Date;
            if (newMinDate != minDate || newMaxDate != maxDate) {
                minDate = newMinDate;
                maxDate = newMaxDate;
                reload();
            }
        }

        private void reload() {
            setActualDateRangeText();

            lista = StatisticsDal.getExpensesSummaryGroupedByCategoryInRange(minDate, maxDate);
            (ColumnChart.Series[0] as PieSeries).ItemsSource = lista;

            expensesToIncomes = StatisticsDal.getExpenseToIncomeComparsion(minDate, maxDate);

            ExpensesToIncomes = makeNewList(expensesToIncomes["expenses"], expensesToIncomes["incomes"]);
            DoughnutChart.ItemsSource = ExpensesToIncomes;
        }
    }
}
