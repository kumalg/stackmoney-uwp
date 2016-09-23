using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Elements {

    public class Operation : OperationPattern {

        public DateTimeOffset? Date { get; set; } 

        public static ObservableCollection<GroupInfoList> GetOperationsGrouped() {

            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();

            GroupInfoList info;
            decimal sumCost = 0;
            DateTimeOffset? dt;

            Settings settings = Dal.GetSettings();

            var query = from item in Dal.GetAllPersons()
                        group item by String.Format("{0:yyyy/MM/dd}", ((DateTimeOffset)item.Date).LocalDateTime) into g
                        orderby g.Key descending
                        select new {
                            GroupName = g.Key,
                            Items = g
                        };

            foreach (var g in query) {
                info = new GroupInfoList();
                info.Key = g.GroupName;

                dt = Convert.ToDateTime(g.GroupName);

                info.dayNum = String.Format("{0:dd}", dt);
                info.day = String.Format("{0:dddd}", dt);
                info.month = String.Format("{0:MMMM yyyy}", dt);

                sumCost = 0;

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


        public static ObservableCollection<CategoryGroupInfoList> GetOperationsByCategoryGrouped() {

            ObservableCollection<CategoryGroupInfoList> groups = new ObservableCollection<CategoryGroupInfoList>();

            string categoryName;
            string categoryIcon;
            string categoryColor;

            CategoryGroupInfoList info;
            decimal sumCost = 0;

            Settings settings = Dal.GetSettings();

            var query = from item in Dal.GetAllPersons()
                        group item by item.CategoryId into g
                        orderby g.Key descending
                        select new {
                            GroupName = g.Key,
                            Items = g
                        };

            foreach (var g in query) {
                info = new CategoryGroupInfoList();

                categoryName = "Bez nazwy";
                categoryIcon = ((TextBlock)Application.Current.Resources["DefaultEllipseIcon"]).Text;
                categoryColor = ((SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"]).Color.ToString();

                foreach (OperationCategory item in Dal.GetAllCategories()) {
                    if (item.Id == g.GroupName) {
                        categoryName = item.Name;
                        categoryIcon = item.Icon;
                        categoryColor = item.Color;
                        break;
                    }
                }

                info.Key = categoryName;
                info.icon = categoryIcon;
                info.color = categoryColor;

                sumCost = 0;

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
