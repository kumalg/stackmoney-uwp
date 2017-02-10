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

    public sealed partial class Statystyki : Page, INotifyPropertyChanged {

        StatisticsData statisticsData = new StatisticsData();
        
        public DateTime minDate = Date.FirstDayInMonth(DateTime.Today);
        public DateTime maxDate = Date.LastDayInMonth(DateTime.Today);

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

        public async Task reload() {
            ProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;

            await Task.Run(() => statisticsData.setNewRangeAndData(minDate, maxDate));

            DateRangeText = statisticsData.getActualDateRangeText(minDate, maxDate);

            ExpensesToIncomes = statisticsData.getExpenseToIncomeComparsion();
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
