using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Finanse.Charts;
using Finanse.Charts.Data;
using Finanse.DataAccessLayer;
using Finanse.Models.Categories;
using Finanse.Models.Extensions;
using Finanse.Models.Operations;

namespace Finanse.Models.Statistics {
    public class StatisticsData {
        private List<Operation> _allOperations;
        public List<Operation> AllOperations {
            get {
                return _allOperations ?? (_allOperations = new List<Operation>());
            }
            private set {
                if (_allOperations != value) {
                    _allOperations = value;
                }
            }
        }

        private IEnumerable<Category> _categoriesAndSubCategories;

        private DateTime _minDate;
        private DateTime _maxDate;

        public StatisticsData() {
            Loaded();
        }

        public async void Loaded() {
            _categoriesAndSubCategories = await CategoriesDal.GetAllCategoriesAndSubCategoriesAsync();
        }

        public async Task SetNewRangeAndData(DateTime minDate, DateTime maxDate) {
            _minDate = minDate;
            _maxDate = maxDate;
            AllOperations = await Dal.GetAllOperationsFromRangeToStatistics(minDate, maxDate).LinkCategories();
        }

        public string GetActualDateRangeText() {
            return _minDate.ToString("d") + "   |   " + _maxDate.ToString("d");
        }

        /*
         * ZAMIAST NA SQL TO NA LISCIE 
         */
         
        public List<ChartDataItem> GetExpensesGroupedByCategoryInRange() {

            var query = from item in AllOperations
                        where item.isExpense
                        group item.Cost by item.Category into g
                        orderby g.Sum() descending
                        select new {
                            Category = g.Key,
                            Cost = g.Sum()
                        };

            decimal sum = query.Sum(item => item.Cost);

            List<ChartDataItem> list = new List<ChartDataItem>();

            list.AddRange(from item in query
                       //   let category = CategoriesDal.GetCategoryByGlobalId(item.CategoryId)
                          select new ChartDataItem(
                              (double) (item.Cost / sum),
                              item.Category.Brush,
                              item.Category.Name, 
                              (double) item.Cost));

            return list;
        }
        
        public List<ChartDataItem> GetIncomesGroupedByCategoryInRange() {
            var query = from item in AllOperations
                        where !item.isExpense
                        group item.Cost by item.Category into g
                        orderby g.Sum() descending
                        select new {
                            Category = g.Key,
                            Cost = g.Sum()
                        };

            decimal sum = query.Sum(item => item.Cost);

            List<ChartDataItem> list = new List<ChartDataItem>();

            list.AddRange(from item in query
                          //let category = CategoriesDal.GetCategoryByGlobalId(item.CategoryId)
                          select new ChartDataItem(
                              (double) (item.Cost / sum), 
                              item.Category.Brush, 
                              item.Category.Name, 
                              (double) item.Cost));

            return list;
        }
       
        public List<SubCategoriesList> GetExpensesFromCategoryGroupedBySubCategoryInRange() {
            List<SubCategoriesList> models = new List<SubCategoriesList>();

            var query = from item in AllOperations
                        where item.isExpense
                        group item by item.Category into g
                        select new {
                            Category = g.Key,
                            SubCategories = from stem in g
                                            group stem.Cost by stem.CategoryGlobalId into gr
                                            orderby gr.Sum() descending
                                            select new {
                                                SubCategoryId = gr.Key,
                                                Cost = gr.Sum()
                                            }
                        };

            foreach (var item in query) {
                if (item.SubCategories.Count() <= 1)
                    continue;

           //     Category category = CategoriesDal.GetCategoryByGlobalId(item.CategoryId);

                SubCategoriesList itemList = new SubCategoriesList {
                    Category = item.Category
                };
                    
                decimal groupySum = item.SubCategories.Sum(i => i.Cost);
                /*
                var subCategories = item.SubCategories.Where(i => i.Cost / groupySum > (decimal) 0.01);
                groupySum = subCategories.Sum(i => i.Cost);
                */
                foreach (var sitem in item.SubCategories) {
                    Category subCategory = _categoriesAndSubCategories.FirstOrDefault(category => category.GlobalId == sitem.SubCategoryId && category is SubCategory);// sitem.SubCategory;//CategoriesDal.GetCategoryByGlobalId(sitem.SubCategoryId);

                    if (subCategory != null)
                        itemList.List.Add(new ChartDataItem(
                            (double)(sitem.Cost / groupySum),
                            subCategory.Brush,
                            subCategory.Name,
                            (double)sitem.Cost
                            ));
                    else {
                        itemList.List.Add(new ChartDataItem(
                            (double)(sitem.Cost / groupySum),
                            item.Category.Brush,
                            new Windows.ApplicationModel.Resources.ResourceLoader().GetString("withoutSubCategory"),
                            (double)sitem.Cost
                            ));
                    }
                }

                models.Add(itemList);
            }

            return models;
        }

        public List<SubCategoriesList> GetIncomesFromCategoryGroupedBySubCategoryInRange() {
            List<SubCategoriesList> models = new List<SubCategoriesList>();

            var query = from item in AllOperations
                        where !item.isExpense
                        group item by item.Category into g
                        select new {
                            Category = g.Key,
                            SubCategories = from stem in g
                                            group stem.Cost by stem.CategoryGlobalId into gr
                                            orderby gr.Sum() descending
                                            select new {
                                                SubCategoryId = gr.Key,
                                                Cost = gr.Sum()
                                            }
                        };

            foreach (var item in query) {
                if (item.SubCategories.Count() <= 1)
                    continue;

               // Category category = CategoriesDal.GetCategoryByGlobalId(item.CategoryId);

                SubCategoriesList itemList = new SubCategoriesList {
                    Category = item.Category
                };

                decimal groupySum = item.SubCategories.Sum(i => i.Cost);

                foreach (var sitem in item.SubCategories) {
                    Category subCategory = _categoriesAndSubCategories.FirstOrDefault(category => category.GlobalId == sitem.SubCategoryId && category is SubCategory);//sitem.SubCategory;

                    if (subCategory != null)
                        itemList.List.Add(new ChartDataItem(
                            (double) (sitem.Cost / groupySum),
                            subCategory.Brush,
                            subCategory.Name,
                            (double) sitem.Cost
                            ));
                    else {
                        itemList.List.Add(new ChartDataItem(
                            (double)(sitem.Cost / groupySum),
                            item.Category.Brush,
                            "Bez podkategorii",
                            (double)sitem.Cost
                            ));
                    }
                }

                models.Add(itemList);
            }

            return models;
        }

        public ObservableCollection<LineChartItem> LineChartTest() {
            int days = (_maxDate - _minDate).Days + 1;
            // = 9 - 7 + 1// 7,8,9

            LineChartItem[] modelss = new LineChartItem[days];
            for (int i = 0; i < modelss.Length; i++) {
                modelss[i] = new LineChartItem {
                    Key = _minDate.AddDays(i).ToString("MM.dd"),
                    Value = 0,
                };
            }

            var query = from item in AllOperations
                       where item.isExpense
                       group item.Cost by item.Date
                       into g
                       select new {
                           Date = DateTime.Parse(g.Key),
                           Cost = g.Sum()
                       };

            foreach (var item in query) {
                int index = days - (_maxDate - item.Date).Days - 1;
                modelss[index] = new LineChartItem {
                    Key = modelss[index].Key,
                    Value = (double)item.Cost,
                };
            }

            return new ObservableCollection<LineChartItem>(modelss);
        }

        public ObservableCollection<LineChartItem> LineChartTest2() {
            int days = (_maxDate - _minDate).Days + 1;
            // = 9 - 7 + 1// 7,8,9

            LineChartItem[] modelss = new LineChartItem[days];
            for (int i = 0; i < modelss.Length; i++) {
                modelss[i] = new LineChartItem {
                    Key = _minDate.AddDays(i).ToString("MM.dd"),
                    Value = 0,
                };
            }

            var query = from item in AllOperations
                       where !item.isExpense
                       group item.Cost by item.Date
                       into g
                       select new {
                           Date = DateTime.Parse(g.Key),
                           Cost = g.Sum()
                       };

            foreach (var item in query) {
                int index = days - (_maxDate - item.Date).Days - 1;
                modelss[index] = new LineChartItem {
                    Key = modelss[index].Key,
                    Value = (double)item.Cost,
                };
            }

            return new ObservableCollection<LineChartItem>(modelss);
        }

        public ObservableCollection<LineChartItem> LineChartTest3() {
            int days = (_maxDate - _minDate).Days + 1;
            // = 9 - 7 + 1// 7,8,9

            LineChartItem[] modelss = new LineChartItem[days];
            for (int i = 0; i < modelss.Length; i++) {
                modelss[i] = new LineChartItem {
                    Key = _minDate.AddDays(i).ToString("MM.dd"),
                    Value = 0,
                };
            }

            var query = from item in AllOperations
                       group item.SignedCost by item.Date
                       into g
                       select new {
                           Date = DateTime.Parse(g.Key),
                           Cost = g.Sum()
                       };

            if (modelss.Length <= 0)
                return new ObservableCollection<LineChartItem>(modelss);
            
            modelss[0].Value = (double)Dal.GetBalanceOfCertainDay(_maxDate);

            for (int i = 1; i < query.Count(); i++) {
                int index = days - (_maxDate - query.ElementAt(i).Date).Days - 1;
                modelss[index].Value += modelss[index - 1].Value + (double)query.ElementAt(i).Cost;
            }

            return new ObservableCollection<LineChartItem>(modelss);
        }

        public List<ChartDataItem> GetExpenseToIncomeComparsion() {
           //ChartData models = new ObservableCollection<ChartPart>();
          List<ChartDataItem> list = new List<ChartDataItem>();

            double expenses = (double)AllOperations.Where(i => i.isExpense).Sum(i => i.Cost);
            double incomes = (double)AllOperations.Where(i => !i.isExpense).Sum(i => i.Cost);

            double sum = expenses + incomes;

            list.Add(new ChartDataItem(
                expenses / sum,
                (SolidColorBrush)Application.Current.Resources["Background+2"],
                "Wydatki",
                expenses
                ));
            list.Add(new ChartDataItem(
                incomes / sum,
                (SolidColorBrush)Application.Current.Resources["AccentColor"],
                "Wpływy",
                incomes
                ));

            return list;
        }
    }
}