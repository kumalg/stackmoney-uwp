using SQLite.Net.Attributes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {

    public class Operation {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Title { get; set; } 
        public int CategoryId { get; set; } 
        public int SubCategoryId { get; set; } 
        public decimal Cost { get; set; } 
        public DateTimeOffset? Date { get; set; } 
        public string ExpenseOrIncome { get; set; } 
        public bool isExpense { get; set; } 
        public string PayForm { get; set; }
        public string MoreInfo { get; set; }

        public string DateToString(DateTimeOffset? Date) {
            string dateString;

            dateString = String.Format("{0:yyyy/MM/dd}", Date);

            return dateString;
        }

        /*
        public string GetIconColor(string myString) {
            myString = "yolo";
            char[] foo = myString.ToCharArray();
            Array.Reverse(foo);
            return new string(foo);
        }
        */

        public static ObservableCollection<GroupInfoList> GetOperationsGrouped() {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();

            string path;
            SQLite.Net.SQLiteConnection conn;

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            Settings settings = conn.Table<Settings>().ElementAt(0);

            var query = from item in conn.Table<Operation>()
                        group item by String.Format("{0:yyyy/MM/dd}", item.Date) into g
                        orderby g.Key descending
                        select new {
                            GroupName = g.Key,
                            Items = g
                        };

            foreach (var g in query) {
                GroupInfoList info = new GroupInfoList();
                info.Key = g.GroupName;

                decimal sumCost = 0;

                foreach (var item in g.Items) {
                    info.Add(item);
                    if (item.isExpense)
                        sumCost -= item.Cost;
                    else
                        sumCost += item.Cost;
                }
                info.cost = sumCost.ToString("C", new CultureInfo(settings.CultureInfoName));
                groups.Add(info);
            }

            return groups;
        }
    }
}
