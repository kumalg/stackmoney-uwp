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

namespace Finanse.Models {
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

        internal ObservableCollection<GroupInfoList<Operation>> GetGroupsByDay(int month, int year, List<int> visiblePayFormList) {

            ObservableCollection<GroupInfoList<Operation>> groups = new ObservableCollection<GroupInfoList<Operation>>();

            GroupInfoList<Operation> info;
            decimal sumCost = 0;
            DateTimeOffset? dt;

            var query = from item in (visiblePayFormList == null) ? Dal.GetAllOperations(month, year) : Dal.GetAllOperations(month, year, visiblePayFormList)
                        group item by item.Date into g
                        orderby Convert.ToDateTime(g.Key) descending
                        select new {
                            GroupName = g.Key,
                            Items = g
                        };
            int dayOfWeek = (int)(new DateTime(year, month, 1).DayOfWeek);
            for (int i = 1; i < dayOfWeek; i++) {
                groups.Add(new GroupInfoList<Operation>());
            }
            for (int i = DateTime.DaysInMonth(year, month); i > 0; i--) {
                groups.Add(new GroupInfoList<Operation>() {
                    Key = year.ToString() + "." + month.ToString("00") + "." + i.ToString("00"),
                    dayNum = i.ToString(),
                });
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

                foreach (var item in g.Items.OrderByDescending(i => i.Id)) {

                    info.Add(item);
                    sumCost += item.isExpense ? -item.Cost : item.Cost;
                }

                info.decimalCost = sumCost;
                info.cost = sumCost.ToString("C", Settings.GetActualCurrency());
                groups[groups.IndexOf(groups.Single(i => i.Key == info.Key))] = info;
            }

            return groups;
        }

        internal ObservableCollection<GroupInfoList<Operation>> GetFutureGroupsByDay(List<int> visiblePayFormList) {

            ObservableCollection<GroupInfoList<Operation>> groups = new ObservableCollection<GroupInfoList<Operation>>();

            GroupInfoList<Operation> info;
            decimal sumCost = 0;
            DateTimeOffset? dt;

            var query = from item in Dal.GetAllFutureOperations(visiblePayFormList)
                        group item by item.Date into g
                        orderby Convert.ToDateTime(g.Key)
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

                foreach (var item in g.Items.OrderByDescending(i => i.Id)) {

                    info.Add(item);
                    sumCost += item.isExpense ? -item.Cost : item.Cost;
                }

                info.decimalCost = sumCost;
                info.cost = sumCost.ToString("C", Settings.GetActualCurrency());
                groups.Add(info);
            }

            return groups;
        }



        internal ObservableCollection<CategoryGroupInfoList<Operation>> GetGroupsByCategory(int month, int year, List<int> visiblePayFormList) {

            ObservableCollection<CategoryGroupInfoList<Operation>> groups = new ObservableCollection<CategoryGroupInfoList<Operation>>();

            string categoryName;
            string categoryIcon;
            string categoryColor;

            CategoryGroupInfoList<Operation> info;
            decimal sumCost = 0;

            //Settings settings = Dal.GetSettings();

            var query = from item in (visiblePayFormList == null) ? Dal.GetAllOperations(month, year) : Dal.GetAllOperations(month, year, visiblePayFormList)
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
                    sumCost += item.isExpense ? -item.Cost : item.Cost;
                }

                info.cost = sumCost.ToString("C", Settings.GetActualCurrency());
                groups.Add(info);
            }

            return groups;
        }

        internal ObservableCollection<CategoryGroupInfoList<Operation>> GetFutureGroupsByCategory(List<int> visiblePayFormList) {

            ObservableCollection<CategoryGroupInfoList<Operation>> groups = new ObservableCollection<CategoryGroupInfoList<Operation>>();

            string categoryName;
            string categoryIcon;
            string categoryColor;

            CategoryGroupInfoList<Operation> info;
            decimal sumCost = 0;

            //Settings settings = Dal.GetSettings();

            var query = from item in Dal.GetAllFutureOperations(visiblePayFormList)
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
                    sumCost += item.isExpense ? -item.Cost : item.Cost;
                }

                info.cost = sumCost.ToString("C", Settings.GetActualCurrency());
                groups.Add(info);
            }

            return groups;
        }
    }
}
