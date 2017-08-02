using Finanse.Charts;
using Finanse.Models;
using Finanse.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Finanse.Charts.Data;
using Finanse.Models.Extensions.DateTimeExtensions;

namespace Finanse.Pages {

    public sealed partial class StatisticsPage : INotifyPropertyChanged {
        private readonly StatisticsData _statisticsData = new StatisticsData();
        
        public DateTime MinDate = DateTime.Today.First();
        public DateTime MaxDate = DateTime.Today;

        private string _incomesPercentageText;
        public string IncomesPercentageText {
            get => _incomesPercentageText;
            set {
                _incomesPercentageText = value;
                RaisePropertyChanged(nameof(IncomesPercentageText));
            }
        }

        private string _dateRangeText;
        public string DateRangeText {
            get => _dateRangeText;
            set {
                _dateRangeText = value;
                RaisePropertyChanged(nameof(DateRangeText));
            }
        }

        private string _expensesValue = string.Empty;
        private string ExpensesValue {
            get => _expensesValue;
            set {
                _expensesValue = value;
                RaisePropertyChanged(nameof(ExpensesValue));
            }
        }

        private string _incomesValue = string.Empty;
        private string IncomesValue {
            get => _incomesValue;
            set {
                _incomesValue = value;
                RaisePropertyChanged(nameof(IncomesValue));
            }
        }

        private List<ChartDataItem> _expensesToIncomes = new List<ChartDataItem>();
        public List<ChartDataItem> ExpensesToIncomes {
            get => _expensesToIncomes;
            set {
                _expensesToIncomes = value;
                RaisePropertyChanged(nameof(ExpensesToIncomes));
            }
        }

        public List<ChartDataItem> ExpensesByCategory { get; set; }
        public List<ChartDataItem> IncomesByCategory { get; set; }
        public List<ChartDataItem> OperationsByCategory => (bool)IncomesRadioButton.IsChecked 
            ? IncomesByCategory 
            : ExpensesByCategory;


        public List<SubCategoriesList> ExpensesFromCategoryGroupedBySubCategory { get; set; }
        public List<SubCategoriesList> IncomesFromCategoryGroupedBySubCategory { get; set; }
        public List<SubCategoriesList> CategoriesGroupedBySubCategories => (bool)IncomesRadioButton.IsChecked 
            ? IncomesFromCategoryGroupedBySubCategory 
            : ExpensesFromCategoryGroupedBySubCategory;
        /*
        private ObservableCollection<SubCategoriesList> categoriesGroupedBySubCategories = new ObservableCollection<SubCategoriesList>();
        public ObservableCollection<SubCategoriesList> CategoriesGroupedBySubCategories {
            get {
                return categoriesGroupedBySubCategories;
            }
            set {
                categoriesGroupedBySubCategories = value;
                RaisePropertyChanged("CategoriesGroupedBySubCategories");
            }
        }*/
        
        private ObservableCollection<LineChartItem> _lineChartTest = new ObservableCollection<LineChartItem>();
        public ObservableCollection<LineChartItem> LineChartTest {
            get => _lineChartTest;
            set {
                _lineChartTest = value;
                RaisePropertyChanged(nameof(LineChartTest));
            }
        }
/*
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
        }*/

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StatisticsPage() {

            InitializeComponent();

            MinDatePicker.Date = MinDate;
            MaxDatePicker.Date = MaxDate;
            
            Reload();
        }


        private void DateRangeFlyout_Closed(object sender, object e) {
            DateTime newMinDate = MinDatePicker.Date.Date;
            DateTime newMaxDate = MaxDatePicker.Date.Date;

            if (newMinDate == MinDate && newMaxDate == MaxDate)
                return;

            MinDate = newMinDate;
            MaxDate = newMaxDate;
            Reload();
        }

        public async Task Reload() {
            ProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;

             await Task.Run(() => _statisticsData.SetNewRangeAndData(MinDate, MaxDate));
           // statisticsData.SetNewRangeAndData(MinDate, MaxDate);

            DateRangeText = _statisticsData.GetActualDateRangeText();
//
            if (!_statisticsData.AllOperations.Any()) {
                ScrollViewer.Visibility = Visibility.Collapsed;
                EmptyListViewInfo.Visibility = Visibility.Visible;
            }
            else {
                ScrollViewer.Visibility = Visibility.Visible;
                EmptyListViewInfo.Visibility = Visibility.Collapsed;
            }
            ExpensesToIncomes = _statisticsData.GetExpenseToIncomeComparsion();
                ExpensesValue = ExpensesToIncomes[0].Value.ToString("C", Settings.ActualCultureInfo);
                IncomesValue = ExpensesToIncomes[1].Value.ToString("C", Settings.ActualCultureInfo);
                //LineChartTest = statisticsData.LineChartTest();
                //   LineChartTest2 = statisticsData.LineChartTest2();
                //  LineChartTest3 = statisticsData.LineChartTest3();

                IncomesPercentageText = (100 * ExpensesToIncomes[1].Part).ToString("0") + "%";



                ExpensesByCategory = _statisticsData.GetExpensesGroupedByCategoryInRange();
                IncomesByCategory = _statisticsData.GetIncomesGroupedByCategoryInRange();
                RaisePropertyChanged(nameof(OperationsByCategory));

                ExpensesFromCategoryGroupedBySubCategory = _statisticsData.GetExpensesFromCategoryGroupedBySubCategoryInRange();
                IncomesFromCategoryGroupedBySubCategory = _statisticsData.GetIncomesFromCategoryGroupedBySubCategoryInRange();
                RaisePropertyChanged(nameof(CategoriesGroupedBySubCategories));
           // }

            /*
            if ((bool)ExpensesRadioButton.IsChecked)
                SetExpensesInGraphs();
            else
                SetIncomesInGraphs();
                */
            ProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void ExpensesRadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            await Task.Delay(5);
            RaisePropertyChanged(nameof(OperationsByCategory));
            RaisePropertyChanged(nameof(CategoriesGroupedBySubCategories));
            //   CategoriesGroupedBySubCategories = statisticsData.GetExpensesFromCategoryGroupedBySubCategoryInRange();
        }

        private async void IncomeRadioButton_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            await Task.Delay(5);
            RaisePropertyChanged(nameof(OperationsByCategory));
            RaisePropertyChanged(nameof(CategoriesGroupedBySubCategories));
            //   CategoriesGroupedBySubCategories = statisticsData.GetIncomesFromCategoryGroupedBySubCategoryInRange();
        }
    }
}
