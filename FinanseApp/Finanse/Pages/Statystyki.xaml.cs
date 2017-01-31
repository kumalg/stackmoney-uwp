using Finanse.DataAccessLayer;
using Finanse.Models;
using Finanse.Models.Statistics;
using FourToolkit.Charts.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace Finanse.Pages {

    public sealed partial class Statystyki : Page, INotifyPropertyChanged {

        StatisticsData statisticsData = new StatisticsData();
        
        public DateTime minDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).Date;
        public DateTime maxDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).Date;

        private string incomesPercentageText;
        public string IncomesPercentageText {
            get {
                return incomesPercentageText;
            }
            set {
                incomesPercentageText = value;
                RaisePropertyChanged("IncomesPercentageText");
            }
        }

        private string dateRangeText;
        public string DateRangeText {
            get {
                return dateRangeText;
            }
            set {
                dateRangeText = value;
                RaisePropertyChanged("DateRangeText");
            }
        }

        private ObservableCollection<ChartPart> expensesToIncomes = new ObservableCollection<ChartPart>();
        public ObservableCollection<ChartPart> ExpensesToIncomes {
            get {
                return expensesToIncomes;
            }
            set {
                expensesToIncomes = value;
                RaisePropertyChanged("ExpensesToIncomes");
            }
        }

        private ObservableCollection<ChartPart> expensesByCategory = new ObservableCollection<ChartPart>();
        public ObservableCollection<ChartPart> ExpensesByCategory {
            get {
                return expensesByCategory;
            }
            set {
                expensesByCategory = value;
                RaisePropertyChanged("ExpensesByCategory");
            }
        }

        private ObservableCollection<ChartPart> incomesByCategory = new ObservableCollection<ChartPart>();
        public ObservableCollection<ChartPart> IncomesByCategory {
            get {
                return incomesByCategory;
            }
            set {
                incomesByCategory = value;
                RaisePropertyChanged("IncomesByCategory");
            }
        }

        private ObservableCollection<ChartPart> categoryExpensesBySubCategory = new ObservableCollection<ChartPart>();
        public ObservableCollection<ChartPart> CategoryExpensesBySubCategory {
            get {
                return categoryExpensesBySubCategory;
            }
            set {
                categoryExpensesBySubCategory = value;
                RaisePropertyChanged("CategoryExpensesBySubCategory");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Statystyki() {

            this.InitializeComponent();

            MinDatePicker.Date = minDate;
            MaxDatePicker.Date = maxDate;

            reload();
        }


        private void DateRangeFlyout_Closed(object sender, object e) {
            DateTime newMinDate = MinDatePicker.Date.Date;
            DateTime newMaxDate = MaxDatePicker.Date.Date;
            if (newMinDate != minDate || newMaxDate != maxDate) {
                minDate = newMinDate;
                maxDate = newMaxDate;
                reload();
            }
        }

        public void reload() {
            DateRangeText = statisticsData.getActualDateRangeText(minDate, maxDate);

            ExpensesToIncomes = statisticsData.setExpensesToIncomesChartValues(minDate, maxDate);
            double incomesPercentage = 100 * ExpensesToIncomes[1].RelativeValue;
            IncomesPercentageText = incomesPercentage.ToString("0") + "%";

            ExpensesByCategory = statisticsData.setRelativeValues(StatisticsDal.getExpensesGroupedByCategoryInRange(minDate, maxDate));
            IncomesByCategory = statisticsData.setRelativeValues(StatisticsDal.getIncomesGroupedByCategoryInRange(minDate, maxDate));
            CategoryExpensesBySubCategory = statisticsData.setRelativeValues(StatisticsDal.getExpensesFromCategoryGroupedBySubCategoryInRange(minDate, maxDate, 17));
        }

    }
}
