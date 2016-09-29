using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Elements {
    public class StoreData {

        private readonly ItemCollection _collection = new ItemCollection();
        public StoreData() {

        }
        /*
        public ItemCollection Collection {
            get {
                return _collection;
            }
        }
        */
        internal ObservableCollection<GroupInfoList<Operation>> GetGroupsByDay(int month, int year) {

            ObservableCollection<GroupInfoList<Operation>> groups = new ObservableCollection<GroupInfoList<Operation>>();

            GroupInfoList<Operation> info;
            decimal sumCost = 0;
            DateTimeOffset? dt;

            var query = from item in Dal.GetAllOperations(month, year)
                        group item by item.Date into g
                        orderby Convert.ToDateTime(g.Key) descending
                        select new {
                            GroupName = g.Key,
                            Items = g
                        };

            foreach (var g in query) {
                info = new GroupInfoList<Operation>() {
                    Key = g.GroupName,
                };
                dt = Convert.ToDateTime(g.GroupName);

                info.dayNum = String.Format("{0:dd}", dt);
                info.day = String.Format("{0:dddd}", dt);
                info.month = String.Format("{0:MMMM yyyy}", dt);

                sumCost = 0;

                foreach (var item in g.Items.OrderByDescending(i=>i.Id)) {
                    info.Add(item);
                    if (item.isExpense)
                        sumCost -= item.Cost;
                    else
                        sumCost += item.Cost;
                }
                info.decimalCost = sumCost;
                info.cost = sumCost.ToString("C", Settings.GetActualCurrency());
                groups.Add(info);
            }

            return groups;
        }

        internal ObservableCollection<CategoryGroupInfoList<Operation>> GetGroupsByCategory(int month, int year) {

            ObservableCollection<CategoryGroupInfoList<Operation>> groups = new ObservableCollection<CategoryGroupInfoList<Operation>>();

            string categoryName;
            string categoryIcon;
            string categoryColor;

            CategoryGroupInfoList<Operation> info;
            decimal sumCost = 0;

            //Settings settings = Dal.GetSettings();

            var query = from item in Dal.GetAllOperations(month, year)
                        group item by item.CategoryId into g
                        orderby g.Key descending
                        select new {
                            GroupName = g.Key,
                            Items = g
                        };

            foreach (var g in query) {
                info = new CategoryGroupInfoList<Operation>();

                categoryName = "Nieprzyporządkowane";
                categoryIcon = ((TextBlock)Application.Current.Resources["DefaultEllipseIcon"]).Text;
                categoryColor = ((SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"]).Color.ToString();
                info.opacity = 0.2;

                foreach (OperationCategory item in Dal.GetAllCategories()) {
                    if (item.Id == g.GroupName) {
                        categoryName = item.Name;
                        categoryIcon = item.Icon;
                        categoryColor = item.Color;
                        info.opacity = 1;
                        break;
                    }
                }

                info.Key = categoryName;
                info.icon = categoryIcon;
                info.color = categoryColor;
                info.iconStyle = new FontFamily(Settings.GetActualIconStyle());

                sumCost = 0;

                foreach (var item in g.Items) {
                    info.Add(item);
                    if (item.isExpense)
                        sumCost -= item.Cost;
                    else
                        sumCost += item.Cost;
                }
                info.cost = sumCost.ToString("C", Settings.GetActualCurrency());
                groups.Add(info);
            }

            return groups;
        }
    }
}
