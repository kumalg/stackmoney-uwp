using Finanse.Charts;
using Finanse.Models;
using Finanse.Models.Helpers;
using Finanse.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Finanse.Pages {

    public sealed partial class StatisticsPage : Page, INotifyPropertyChanged {

        StatisticsData statisticsData = new StatisticsData();
        
        public DateTime minDate = Date.FirstDayInMonth(DateTime.Today);
        public DateTime maxDate = DateTime.Today;

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

        private string expensesValue = string.Empty;
        private string ExpensesValue {
            get {
                return expensesValue;
            }
            set {
                expensesValue = value;
                RaisePropertyChanged("ExpensesValue");
            }
        }

        private string incomesValue = string.Empty;
        private string IncomesValue {
            get {
                return incomesValue;
            }
            set {
                incomesValue = value;
                RaisePropertyChanged("IncomesValue");
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

        private ObservableCollection<ChartPart> operationsByCategory = new ObservableCollection<ChartPart>();
        public ObservableCollection<ChartPart> OperationsByCategory {
            get {
                return operationsByCategory;
            }
            set {
                operationsByCategory = value;
                RaisePropertyChanged("OperationsByCategory");
            }
        }

        private ObservableCollection<SubCategoriesList> categoriesGroupedBySubCategories = new ObservableCollection<SubCategoriesList>();
        public ObservableCollection<SubCategoriesList> CategoriesGroupedBySubCategories {
            get {
                return categoriesGroupedBySubCategories;
            }
            set {
                categoriesGroupedBySubCategories = value;
                RaisePropertyChanged("CategoriesGroupedBySubCategories");
            }
        }

        private ObservableCollection<LineChartItem> lineChartTest = new ObservableCollection<LineChartItem>();
        public ObservableCollection<LineChartItem> LineChartTest {
            get {
                return lineChartTest;
            }
            set {
                lineChartTest = value;
                RaisePropertyChanged("LineChartTest");
            }
        }

        private ObservableCollection<LineChartItem> lineChartTest2 = new ObservableCollection<LineChartItem>();
        public ObservableCollection<LineChartItem> LineChartTest2 {
            get {
                return lineChartTest2;
            }
            set {
                lineChartTest2 = value;
                RaisePropertyChanged("LineChartTest2");
            }
        }

        private ObservableCollection<LineChartItem> lineChartTest3 = new ObservableCollection<LineChartItem>();
        public ObservableCollection<LineChartItem> LineChartTest3 {
            get {
                return lineChartTest3;
            }
            set {
                lineChartTest3 = value;
                RaisePropertyChanged("LineChartTest3");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public StatisticsPage() {

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

        public async Task reload() {
            ProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;

            await Task.Run(() => statisticsData.setNewRangeAndData(minDate, maxDate));

            DateRangeText = statisticsData.getActualDateRangeText(minDate, maxDate);

            ExpensesToIncomes = statisticsData.getExpenseToIncomeComparsion();
            ExpensesValue = ExpensesToIncomes[0].UnrelativeValue.ToString("C", Settings.getActualCultureInfo());
            IncomesValue = ExpensesToIncomes[1].UnrelativeValue.ToString("C", Settings.getActualCultureInfo());
            LineChartTest = statisticsData.lineChartTest();
            LineChartTest2 = statisticsData.lineChartTest2();
            LineChartTest3 = statisticsData.lineChartTest3();

            IncomesPercentageText = (100 * ExpensesToIncomes[1].RelativeValue).ToString("0") + "%";

            if ((bool)ExpensesRadioButton.IsChecked)
                setExpensesInGraphs();
            else
                setIncomesInGraphs();

            ProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void ExpensesRadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            await Task.Delay(5);
            setExpensesInGraphs();
        }

        private async void IncomeRadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            await Task.Delay(5);
            setIncomesInGraphs();
        }

        private async Task setExpensesInGraphs() {
            OperationsByCategory = statisticsData.getExpensesGroupedByCategoryInRange(minDate, maxDate);
            CategoriesGroupedBySubCategories = statisticsData.getExpensesFromCategoryGroupedBySubCategoryInRange();
        }

        private async Task setIncomesInGraphs() {
            OperationsByCategory = statisticsData.getIncomesGroupedByCategoryInRange(minDate, maxDate);
            CategoriesGroupedBySubCategories = statisticsData.getIncomesFromCategoryGroupedBySubCategoryInRange();
        }
    }
}
