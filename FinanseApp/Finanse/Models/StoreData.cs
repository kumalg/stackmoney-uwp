﻿using Finanse.DataAccessLayer;
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
        private int month, year;
        private bool isFuture;
        private List<int> visiblePayFormList;
        //private List<int> visiblePayFormList;
        private readonly ItemCollection _collection = new ItemCollection();
        public StoreData(int month, int year, bool isFuture, List<int> visiblePayFormList) {

            this.month = month;
            this.year = year;
            this.isFuture = isFuture;
            this.visiblePayFormList = visiblePayFormList;
        }

        public void SetVisiblePayFormList(List<int> visiblePayFormList) {
            if(this.visiblePayFormList != visiblePayFormList)
                this.visiblePayFormList = visiblePayFormList;
        }

        private ObservableCollection<GroupInfoList<Operation>> groupsByDay = null;

        public ObservableCollection<GroupInfoList<Operation>> GroupsByDay {
            get {
                if (groupsByDay == null || visiblePayFormList != null) {

                    groupsByDay = new ObservableCollection<GroupInfoList<Operation>>();

                    GroupInfoList<Operation> info;
                    decimal sumCost = 0;
                    DateTimeOffset? dt;

                    var query = from item in isFuture ? Dal.GetAllFutureOperations(visiblePayFormList) : Dal.GetAllOperations(month, year, visiblePayFormList)
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

                        foreach (var item in g.Items.OrderByDescending(i => i.Id)) {

                            info.Add(item);
                            sumCost += item.isExpense ? -item.Cost : item.Cost;
                        }

                        info.decimalCost = sumCost;
                        info.cost = sumCost.ToString("C", Settings.GetActualCurrency());
                        groupsByDay.Add(info);
                    }
                }
                return groupsByDay;
            }
        }
        /*
        public ObservableCollection<GroupInfoList<Operation>> GroupsByDaySelectedAccounts(List<int> visiblePayFormList) {

            if (visiblePayFormList == null)
                return GroupsByDay;

            ObservableCollection<GroupInfoList<Operation>> groups = new ObservableCollection<GroupInfoList<Operation>>();

            GroupInfoList<Operation> info;
            decimal sumCost = 0;
            DateTimeOffset? dt;

            var query = from item in isFuture ? Dal.GetAllFutureOperations(visiblePayFormList) : Dal.GetAllOperations(month, year, visiblePayFormList)
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
        */
        List<HeaderItem> operationHeaders = null;
        public List<HeaderItem> OperationHeaders {
            get {
                if (operationHeaders == null || visiblePayFormList != null) {
                    operationHeaders = new List<HeaderItem>();

                    int dayOfWeek = (int)(new DateTime(year, month, 1).DayOfWeek);
                    for (int i = 1; i < dayOfWeek; i++) {
                        operationHeaders.Add(new HeaderItem() { Day = String.Empty, IsEnabled = false });
                    }

                    for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++) {

                        if (this.GroupsByDay.Any(k => Convert.ToDateTime(k.Key).Day.ToString() == i.ToString()))
                            operationHeaders.Add(new HeaderItem() { Day = i.ToString(), IsEnabled = true });
                        else
                            operationHeaders.Add(new HeaderItem() { Day = i.ToString(), IsEnabled = false });
                    }
                }

                return operationHeaders;
            }
        }
        /*
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
        */


        private ObservableCollection<CategoryGroupInfoList<Operation>> groupsByCategory = null;

        public ObservableCollection<CategoryGroupInfoList<Operation>> GroupsByCategory {
            get {
                if (groupsByCategory == null || visiblePayFormList != null) {

                    groupsByCategory = new ObservableCollection<CategoryGroupInfoList<Operation>>();

                    string categoryName;
                    string categoryIcon;
                    string categoryColor;

                    CategoryGroupInfoList<Operation> info;
                    decimal sumCost = 0;

                    //Settings settings = Dal.GetSettings();

                    var query = from item in isFuture ? Dal.GetAllFutureOperations(visiblePayFormList) : Dal.GetAllOperations(month, year, visiblePayFormList)
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
                        groupsByCategory.Add(info);
                    }
                }
                return groupsByCategory;
            }
        }
        /*
        public ObservableCollection<CategoryGroupInfoList<Operation>> GroupsByCategorySelectedAccounts(List<int> visiblePayFormList) {

            if (visiblePayFormList == null)
                return GroupsByCategory;

            ObservableCollection<CategoryGroupInfoList<Operation>> groups = new ObservableCollection<CategoryGroupInfoList<Operation>>();

            string categoryName;
            string categoryIcon;
            string categoryColor;

            CategoryGroupInfoList<Operation> info;
            decimal sumCost = 0;

            //Settings settings = Dal.GetSettings();

            var query = from item in isFuture ? Dal.GetAllFutureOperations(visiblePayFormList) : Dal.GetAllOperations(month, year, visiblePayFormList)
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

        internal ObservableCollection<CategoryGroupInfoList<Operation>> GetGroupsByCategory(int month, int year, List<int> visiblePayFormList) {

            ObservableCollection<CategoryGroupInfoList<Operation>> groups = new ObservableCollection<CategoryGroupInfoList<Operation>>();

            string categoryName;
            string categoryIcon;
            string categoryColor;

            CategoryGroupInfoList<Operation> info;
            decimal sumCost = 0;

            //Settings settings = Dal.GetSettings();

            var query = from item in Dal.GetAllOperations(month, year, visiblePayFormList)
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
        */
    }
}
