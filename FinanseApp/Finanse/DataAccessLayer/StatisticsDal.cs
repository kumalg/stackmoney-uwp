using Finanse.Models;
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

        public static SortedDictionary<string, double> getExpensesSummaryGroupedByCategoryInRange(DateTime minDate, DateTime maxDate) {
            SortedDictionary<string, double> models = new SortedDictionary<string, double>();

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                var query = from item in db.Table<Operation>().ToList() where 
                            !String.IsNullOrEmpty(item.Date)
                            && item.isExpense
                            && isDateInRange(item.Date, minDate, maxDate)
                            group item.Cost by item.CategoryId into g
                            orderby g.Key descending
                            select new {
                                CategoryId = Dal.getCategoryNameById(g.Key),
                                Cost = g.Sum()
                            };

                foreach (var item in query)
                    models.Add(item.CategoryId, (double)item.Cost);
            }

            return models;
        }

        public static Dictionary<string, double> getExpenseToIncomeComparsion(DateTime minDate, DateTime maxDate) {
            Dictionary<string, double> models = new Dictionary<string, double>();

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

                models.Add("expenses", expenses);
                models.Add("incomes", incomes);
            }

            return models;
        }

        private static bool isDateInRange(string dateString, DateTime minDate, DateTime maxDate) {
            DateTime date = Convert.ToDateTime(dateString);
            return date.Date >= minDate.Date && date <= maxDate.Date;
        }
    }
}
