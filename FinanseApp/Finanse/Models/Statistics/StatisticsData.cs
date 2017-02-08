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
            AllOperations = Dal.getAllOperationsFromRangeToStatistics(minDate, maxDate);
        }

        public string getActualDateRangeText(DateTime minDate, DateTime maxDate) {
            return minDate.ToString("d") + "   |   " + maxDate.ToString("d");
        }

        /*
         * ZAMIAST NA SQL TO NA LISCIE 
         */

        public ObservableCollection<ChartPart> getExpensesGroupedByCategoryInRange(DateTime minDate, DateTime maxDate) {
            ObservableCollection<ChartPart> models = new ObservableCollection<ChartPart>();

            var query = from item in AllOperations
                        where item.isExpense
                        group item.Cost by item.CategoryId into g
                        orderby g.Sum() descending
                        select new {
                            CategoryId = g.Key,
                            Cost = g.Sum()
                        };

            decimal sum = query.Sum(item => item.Cost);

            foreach (var item in query) {
                OperationCategory operationCategory = Dal.getOperationCategoryById(item.CategoryId);
                models.Add(new ChartPart {
                    SolidColorBrush = operationCategory.Color,
                    Name = operationCategory.Name,
                    Tag = item.CategoryId,
                    UnrelativeValue = (double)item.Cost,
                    RelativeValue = (double)(item.Cost / sum),
                });
            }

            return models;
        }

        public ObservableCollection<ChartPart> getIncomesGroupedByCategoryInRange(DateTime minDate, DateTime maxDate) {
            ObservableCollection<ChartPart> models = new ObservableCollection<ChartPart>();

            var query = from item in AllOperations
                        where !item.isExpense
                        group item.Cost by item.CategoryId into g
                        orderby g.Sum() descending
                        select new {
                            CategoryId = g.Key,
                            Cost = g.Sum()
                        };

            decimal sum = query.Sum(item => item.Cost);

            foreach (var item in query) {
                OperationCategory operationCategory = Dal.getOperationCategoryById(item.CategoryId);
                models.Add(new ChartPart {
                    SolidColorBrush = operationCategory.Color,
                    Name = operationCategory.Name,
                    Tag = item.CategoryId,
                    UnrelativeValue = (double)item.Cost,
                    RelativeValue = (double)(item.Cost / sum),
                });
            }

            return models;
        }
       
        public ObservableCollection<SubCategoriesList> getExpensesFromCategoryGroupedBySubCategoryInRange() {
            ObservableCollection<SubCategoriesList> models = new ObservableCollection<SubCategoriesList>();

            var query = from item in AllOperations
                        where item.isExpense
                        group item by item.CategoryId into g
                        select new {
                            CategoryId = g.Key,
                            SubCategories = from stem in g
                                            group stem.Cost by stem.SubCategoryId into gr
                                            orderby gr.Sum() descending
                                            select new {
                                                SubCategoryId = gr.Key,
                                                Cost = gr.Sum()
                                            }
                        };

            foreach (var item in query) {
                if (item.SubCategories.Count() > 1) {
                    OperationCategory operationCategory = Dal.getOperationCategoryById(item.CategoryId);

                    SubCategoriesList itemList = new SubCategoriesList {
                        Name = operationCategory.Name
                    };
                    
                    decimal groupySum = item.SubCategories.Sum(i => i.Cost);

                    foreach (var sitem in item.SubCategories) {
                        OperationSubCategory operationSubCategory = Dal.getOperationSubCategoryById(sitem.SubCategoryId);
                        if (operationSubCategory != null)
                            itemList.List.Add(new ChartPart {
                                SolidColorBrush = operationSubCategory.Color,
                                Name = operationSubCategory.Name,
                                UnrelativeValue = (double)sitem.Cost,
                                RelativeValue = (double)(sitem.Cost / groupySum)
                            });
                        else {
                            itemList.List.Add(new ChartPart {
                                SolidColorBrush = operationCategory.Color,
                                Name = "Bez podkategorii",
                                UnrelativeValue = (double)sitem.Cost,
                                RelativeValue = (double)(sitem.Cost / groupySum)
                            });
                        }
                    }

                    models.Add(itemList);
                }
            }

            return models;
        }

        public ObservableCollection<SubCategoriesList> getIncomesFromCategoryGroupedBySubCategoryInRange() {
            ObservableCollection<SubCategoriesList> models = new ObservableCollection<SubCategoriesList>();

            var query = from item in AllOperations
                        where !item.isExpense
                        group item by item.CategoryId into g
                        select new {
                            CategoryId = g.Key,
                            SubCategories = from stem in g
                                            group stem.Cost by stem.SubCategoryId into gr
                                            orderby gr.Sum() descending
                                            select new {
                                                SubCategoryId = gr.Key,
                                                Cost = gr.Sum()
                                            }
                        };

            foreach (var item in query) {
                if (item.SubCategories.Count() > 1) {
                    OperationCategory operationCategory = Dal.getOperationCategoryById(item.CategoryId);

                    SubCategoriesList itemList = new SubCategoriesList {
                        Name = operationCategory.Name
                    };

                    decimal groupySum = item.SubCategories.Sum(i => i.Cost);

                    foreach (var sitem in item.SubCategories) {
                        OperationSubCategory operationSubCategory = Dal.getOperationSubCategoryById(sitem.SubCategoryId);
                        if (operationSubCategory != null)
                            itemList.List.Add(new ChartPart {
                                SolidColorBrush = operationSubCategory.Color,
                                Name = operationSubCategory.Name,
                                UnrelativeValue = (double)sitem.Cost,
                                RelativeValue = (double)(sitem.Cost / groupySum)
                            });
                        else {
                            itemList.List.Add(new ChartPart {
                                SolidColorBrush = operationCategory.Color,
                                Name = "Bez podkategorii",
                                UnrelativeValue = (double)sitem.Cost,
                                RelativeValue = (double)(sitem.Cost / groupySum)
                            });
                        }
                    }

                    models.Add(itemList);
                }
            }

            return models;
        }


        public ObservableCollection<ChartPart> getExpenseToIncomeComparsion() {
            ObservableCollection<ChartPart> models = new ObservableCollection<ChartPart>();

            double expenses = (double)AllOperations.Where(i => i.isExpense).Sum(i => i.Cost);
            double incomes = (double)AllOperations.Where(i => !i.isExpense).Sum(i => i.Cost);
            double sum = expenses + incomes;

            models.Add(new ChartPart {
                Name = "Wydatki",
                RelativeValue = expenses / sum,
                UnrelativeValue = expenses,
                SolidColorBrush = (SolidColorBrush)Application.Current.Resources["Background+2"]
            });

            models.Add(new ChartPart {
                Name = "Wpływy",
                RelativeValue = incomes / sum,
                UnrelativeValue = incomes,
                SolidColorBrush = (SolidColorBrush)Application.Current.Resources["AccentColor"]
            });

            return models;
        }
    }
}
