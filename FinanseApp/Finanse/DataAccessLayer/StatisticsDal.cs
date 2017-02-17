using Finanse.Models;
using Finanse.Models.Statistics;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Finanse.DataAccessLayer {
    class StatisticsDal {
        private static string dbPath = string.Empty;
        private static string DbPath {
            get {
                if (string.IsNullOrEmpty(dbPath)) {
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                    //    dbPath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "db.sqlite");
                }

                return dbPath;
            }
        }

        private static SQLiteConnection DbConnection {
            get {
                return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            }
        }

        public static List<ChartPart> getExpensesGroupedByCategoryInRange(DateTime minDate, DateTime maxDate) {
            List<ChartPart> models = new List<ChartPart>();

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                var query = from item in db.Table<Operation>().ToList()
                            where !String.IsNullOrEmpty(item.Date) 
                            && item.isExpense 
                            && isDateInRange(item.Date, minDate, maxDate)
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
            }

            return models;
        }

        public static List<ChartPart> getIncomesGroupedByCategoryInRange(DateTime minDate, DateTime maxDate) {
            List<ChartPart> models = new List<ChartPart>();

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                var query = from item in db.Table<Operation>().ToList()
                            where !String.IsNullOrEmpty(item.Date)
                            && !item.isExpense
                            && isDateInRange(item.Date, minDate, maxDate)
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
            }

            return models;
        }

        public static List<ChartPart> getExpensesFromCategoryGroupedBySubCategoryInRange(DateTime minDate, DateTime maxDate, int CategoryId) {
            List<ChartPart> models = new List<ChartPart>();

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                var query = from item in db.Table<Operation>().ToList()
                            where !String.IsNullOrEmpty(item.Date)
                            && item.isExpense
                            && isDateInRange(item.Date, minDate, maxDate)
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
                            Name = operationCategory.Name,
                            UnrelativeValue = (double)item.Cost
                        });
                    }
                }
            }

            return models;
        }

        public static List<ChartPart> lineChartTest(DateTime minDate, DateTime maxDate) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                List<ChartPart> models = new List<ChartPart>();
                var query = db.Query<Operation>("SELECT * FROM Operation WHERE Date >= ? AND Date <= ?", minDate.ToString("yyyy.MM.dd"), maxDate.ToString("yyyy.MM.dd"));

                var dupa = from item in query
                           group item.Cost by item.Date
                           into g select g.Sum();

                for (int i = 0; i < dupa.Count(); i++) {
                    models.Add(new ChartPart {
                        Name = (i + 1).ToString(),
                        RelativeValue = (double)dupa.ElementAt(i),
                        UnrelativeValue = (double)dupa.ElementAt(i)
                    });
                }

                return models;
            }
        }

        public static List<double> getExpenseToIncomeComparsion(DateTime minDate, DateTime maxDate) {
            List<double> models = new List<double>();

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                double expenses = (double)(from item in db.Table<Operation>().ToList()
                            where !String.IsNullOrEmpty(item.Date)
                            && item.isExpense
                            && isDateInRange(item.Date, minDate, maxDate)
                            select item.Cost).Sum();

                double incomes = (double)(from item in db.Table<Operation>().ToList()
                                     where !String.IsNullOrEmpty(item.Date)
                                     && !item.isExpense
                                     && isDateInRange(item.Date, minDate, maxDate)
                                     select item.Cost).Sum();

                models.Add(expenses);
                models.Add(incomes);
            }

            return models;
        }

        private static bool isDateInRange(string dateString, DateTime minDate, DateTime maxDate) {
            DateTime date = Convert.ToDateTime(dateString);
            return date.Date >= minDate.Date && date <= maxDate.Date;
        }
    }
}
