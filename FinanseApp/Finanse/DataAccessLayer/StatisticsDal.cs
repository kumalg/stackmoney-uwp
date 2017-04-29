using Finanse.Models.Statistics;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Linq;
using Finanse.Models.Categories;
using Finanse.Models.Operations;

namespace Finanse.DataAccessLayer {
    class StatisticsDal : DalBase {

        public static List<ChartPart> GetExpensesFromCategoryGroupedBySubCategoryInRange(DateTime minDate, DateTime maxDate, string categoryGlobalId) {
            List<ChartPart> models = new List<ChartPart>();

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                var query = from item in db.Table<Operation>().ToList()
                            where !String.IsNullOrEmpty(item.Date)
                            && item.isExpense
                            && IsDateInRange(item.Date, minDate, maxDate)
                            && item.CategoryGlobalId == categoryGlobalId
                            group item.Cost by item.SubCategory?.GlobalId into g
                            orderby g.Sum() descending
                            select new {
                                SubCategoryId = g.Key,
                                Cost = g.Sum()
                            };

                foreach (var item in query) {
                    SubCategory subCategory = CategoriesDal.GetCategoryByGlobalId(item.SubCategoryId) as SubCategory;
                    if (subCategory != null)
                        models.Add(new ChartPart {
                            SolidColorBrush = subCategory.Brush,
                            Name = subCategory.Name,
                            UnrelativeValue = (double)item.Cost
                        });
                    else {
                        Category category = CategoriesDal.GetCategoryByGlobalId(categoryGlobalId);
                        models.Add(new ChartPart {
                            SolidColorBrush = category.Brush,
                            Name = category.Name,
                            UnrelativeValue = (double)item.Cost
                        });
                    }
                }
            }
            
            return models;
        }


        private static bool IsDateInRange(string dateString, DateTime minDate, DateTime maxDate) {
            DateTime date = Convert.ToDateTime(dateString);
            return date.Date >= minDate.Date && date <= maxDate.Date;
        }
    }
}
