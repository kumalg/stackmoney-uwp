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

        public List<Operation> AllOperations = new List<Operation>();
        /*
        public StatisticsData(List<Operation> AllOperations) {
            this.AllOperations = AllOperations;
        }*/

        public void setNewRangeAndData(DateTime minDate, DateTime maxDate) {
            AllOperations = Dal.getAllOperationsFromRange(minDate, maxDate);
        }

        public ObservableCollection<ChartPart> setExpensesToIncomesChartValues(DateTime minDate, DateTime maxDate) {
            ObservableCollection<double> expenseToIncomeComparsion = getExpenseToIncomeComparsion(minDate, maxDate);
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

                string percentagePart = (part.RelativeValue > 0.0001) ?
                    (part.RelativeValue * 100).ToString("0.##") : "<0.01";
            }

            return collection;
        }

        public string getActualDateRangeText(DateTime minDate, DateTime maxDate) {
            return minDate.ToString("d") + "   |   " + maxDate.ToString("d");
        }

        /*
         * ZAMIAST NA SQL TO NA LISCIE 
         */

        public ObservableCollection<ChartPart> getExpensesGroupedByCategoryInRange(DateTime minDate, DateTime maxDate) {
            List<ChartPart> models = new List<ChartPart>();

            var query = from item in AllOperations
                        where item.isExpense
                        group item.Cost by item.CategoryId into g
                        orderby g.Sum() descending
                        select new {
                            CategoryId = g.Key,
                            Cost = g.Sum()
                        };

            foreach (var item in query) {
                OperationCategory operationCategory = Dal.getOperationCategoryById(item.CategoryId);
                models.Add(new ChartPart {
                    SolidColorBrush = operationCategory.Color,
                    Name = operationCategory.Name,
                    Tag = item.CategoryId,
                    UnrelativeValue = (double)item.Cost
                });
            }

            return setRelativeValues(models);
        }

        public ObservableCollection<ChartPart> getIncomesGroupedByCategoryInRange(DateTime minDate, DateTime maxDate) {
            List<ChartPart> models = new List<ChartPart>();

            var query = from item in AllOperations
                        where !item.isExpense
                        group item.Cost by item.CategoryId into g
                        orderby g.Sum() descending
                        select new {
                            CategoryId = g.Key,
                            Cost = g.Sum()
                        };

            foreach (var item in query) {
                OperationCategory operationCategory = Dal.getOperationCategoryById(item.CategoryId);
                models.Add(new ChartPart {
                    SolidColorBrush = operationCategory.Color,
                    Name = operationCategory.Name,
                    Tag = item.CategoryId,
                    UnrelativeValue = (double)item.Cost
                });
            }

            return setRelativeValues(models);
        }

        public ObservableCollection<ChartPart> getExpensesFromCategoryGroupedBySubCategoryInRange(DateTime minDate, DateTime maxDate, int CategoryId) {
            List<ChartPart> models = new List<ChartPart>();

            var query = from item in AllOperations
                        where item.isExpense
                        && item.CategoryId == CategoryId
                        group item.Cost by item.SubCategoryId into g
                        orderby g.Sum() descending
                        select new {
                            SubCategoryId = g.Key,
                            Cost = g.Sum()
                        };

            foreach (var item in query) {
                
                OperationSubCategory operationSubCategory = Dal.getOperationSubCategoryById(item.SubCategoryId);
                if (operationSubCategory != null)
                    models.Add(new ChartPart {
                        SolidColorBrush = operationSubCategory.Color,
                        Name = operationSubCategory.Name,
                        UnrelativeValue = (double)item.Cost
                    });
                else {
                    OperationCategory operationCategory = Dal.getOperationCategoryById(CategoryId);
                    models.Add(new ChartPart {
                        SolidColorBrush = operationCategory.Color,
                        Name = "Bez podkategorii",
                        UnrelativeValue = (double)item.Cost
                    });
                }
            }

            return setRelativeValues(models);
        }

        public ObservableCollection<double> getExpenseToIncomeComparsion(DateTime minDate, DateTime maxDate) {
            ObservableCollection<double> models = new ObservableCollection<double>();

            double expenses = (double)AllOperations.Where(i => i.isExpense).Sum(i => i.Cost);
            double incomes = (double)AllOperations.Where(i => !i.isExpense).Sum(i => i.Cost);

            models.Add(expenses);
            models.Add(incomes);

            return models;
        }
    }
}
