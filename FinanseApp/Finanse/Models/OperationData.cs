using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class OperationData {
        private int month, year;
        private bool isFuture;
        private HashSet<int> visiblePayFormList;

        private readonly ItemCollection _collection = new ItemCollection();
        public OperationData(int month, int year, bool isFuture, HashSet<int> visiblePayFormList) {
            this.month = month;
            this.year = year;
            this.isFuture = isFuture;
            this.visiblePayFormList = visiblePayFormList;
        }

        public void SetVisiblePayFormList(HashSet<int> visiblePayFormList) {
            if (this.visiblePayFormList != visiblePayFormList)
                this.visiblePayFormList = visiblePayFormList;
        }

        private ObservableCollection<GroupInfoList<Operation>> groupsByDay = null;

        public ObservableCollection<GroupInfoList<Operation>> GroupsByDay {
            get {
                if (groupsByDay == null || visiblePayFormList != null) {

                    groupsByDay = new ObservableCollection<GroupInfoList<Operation>>();

                    GroupInfoList<Operation> info;
                    
                    var query = from item in isFuture ? Dal.getAllFutureOperations(visiblePayFormList) : Dal.getAllOperations(month, year, visiblePayFormList)
                                group item by item.Date into g
                                orderby g.Key descending
                                select new {
                                    GroupName = g.Key,
                                    Items = g
                                };

                    foreach (var g in query) {
                        info = new GroupInfoList<Operation>() {
                            Key = new GroupHeaderByDay(g.GroupName),
                        };
                        
                        foreach (var item in g.Items.OrderByDescending(i => i.Id))
                            info.Add(item);
                        
                        groupsByDay.Add(info);
                    }
                }
                return groupsByDay;
            }
        }

        private int howManyEmptyCells(int year, int month) {
            int dayOfWeek = (int)(new DateTime(year, month, 1).DayOfWeek) - (int)Settings.getFirstDayOfWeek();
            if (dayOfWeek < 1)
                dayOfWeek += 7;
            return dayOfWeek;
        }

        List<HeaderItem> operationHeaders = null;
        public List<HeaderItem> OperationHeaders {
            get {
                if (operationHeaders == null || visiblePayFormList != null) {
                    operationHeaders = new List<HeaderItem>();

                    int dayOfWeek = howManyEmptyCells(year, month);
                    /*
                    string[] names = Settings.getActualCultureInfo().DateTimeFormat.AbbreviatedDayNames;
                    string monday = names[(int)DayOfWeek.Monday];

                    operationHeaders.Add(new HeaderItem() { Day = names[(int)DayOfWeek.Monday], IsEnabled = false });
                    operationHeaders.Add(new HeaderItem() { Day = names[(int)DayOfWeek.Tuesday], IsEnabled = false });
                    operationHeaders.Add(new HeaderItem() { Day = names[(int)DayOfWeek.Wednesday], IsEnabled = false });
                    operationHeaders.Add(new HeaderItem() { Day = names[(int)DayOfWeek.Thursday], IsEnabled = false });
                    operationHeaders.Add(new HeaderItem() { Day = names[(int)DayOfWeek.Friday], IsEnabled = false });
                    operationHeaders.Add(new HeaderItem() { Day = names[(int)DayOfWeek.Saturday], IsEnabled = false });
                    operationHeaders.Add(new HeaderItem() { Day = names[(int)DayOfWeek.Sunday], IsEnabled = false });
                    */
                    for (int i = 0; i < dayOfWeek; i++)
                        operationHeaders.Add(new HeaderItem() { Day = String.Empty, IsEnabled = false });

                    for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++) {

                        if (this.groupsByDay.Any(k => ((GroupHeaderByDay)k.Key).dayNum == i.ToString()))
                            operationHeaders.Add(new HeaderItem() { Day = i.ToString(), IsEnabled = true });
                        else
                            operationHeaders.Add(new HeaderItem() { Day = i.ToString(), IsEnabled = false });
                    }
                }

                return operationHeaders;
            }
        }

        private ObservableCollection<GroupInfoList<Operation>> groupsByCategory = null;

        public ObservableCollection<GroupInfoList<Operation>> GroupsByCategory {
            get {
                if (groupsByCategory == null || visiblePayFormList != null) {

                    groupsByCategory = new ObservableCollection<GroupInfoList<Operation>>();

                    GroupInfoList<Operation> info;
                    
                    var query = from item in isFuture ? Dal.getAllFutureOperations(visiblePayFormList) : Dal.getAllOperations(month, year, visiblePayFormList)
                                group item by item.CategoryId into g
                                orderby g.Key descending
                                select new {
                                    GroupName = g.Key,
                                    Items = g
                                };

                    foreach (var g in query) {
                        info = new GroupInfoList<Operation>();

                        info.Key = new GroupHeaderByCategory {
                            //name = "Nieprzyporządkowane", /// WAŻNE MAXXXXXXXXXXXXXX
                            categoryId = -1,
                            icon = ((FontIcon)Application.Current.Resources["DefaultEllipseIcon"]).Glyph,
                            color = ((SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"]).Color.ToString(),
                            opacity = 0.2,
                        };

                        foreach (OperationCategory item in Dal.getAllCategories()) {
                            if (item.Id == g.GroupName) {
                                ((GroupHeaderByCategory)info.Key).categoryId = item.Id;
                                ((GroupHeaderByCategory)info.Key).icon = item.Icon.Glyph;
                                ((GroupHeaderByCategory)info.Key).color = item.Color.ToString(); /// cymczasowe
                                ((GroupHeaderByCategory)info.Key).opacity = 1;
                                break;
                            }
                        }

                        ((GroupHeaderByCategory)info.Key).iconStyle = new FontFamily(Settings.getActualIconStyle());

                        foreach (var item in g.Items.OrderByDescending(i=>i.Id))
                            info.Add(item);

                        groupsByCategory.Add(info);
                    }
                }
                return groupsByCategory;
            }
        }
    }
}
