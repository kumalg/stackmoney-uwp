﻿using Finanse.DataAccessLayer;
using Finanse.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class OperationData : INotifyPropertyChanged {

        private DateTime monthOfDayGrouping;
        private DateTime monthOfCategoryGrouping;
        private HashSet<int> visiblePayFormListOfDayGrouping = new HashSet<int>();
        private HashSet<int> visiblePayFormListOfCategoryGrouping;


        private DateTime actualMonth = Date.FirstDayInMonth(DateTime.Today);
        public DateTime ActualMonth {
            get {
                return actualMonth;
            }
            set {
                actualMonth = value;
                OnPropertyChanged("ActualMonthText");
                OnPropertyChanged("ActualOperationsSum");
                SetNewOperationsList();

                OnPropertyChanged("OperationsByDay");
            }
        }


        public string ActualMonthText {
            get {
                string actualMonthText = string.Empty;
                if (!IsFuture) {
                    actualMonthText = DateTimeFormatInfo.CurrentInfo.GetMonthName(ActualMonth.Month).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(ActualMonth.Month).Substring(1);
                    if (ActualMonth.Year != DateTime.Today.Year)
                        actualMonthText += " " + ActualMonth.Year.ToString();
                }
                else {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    actualMonthText = loader.GetString("plannedString");
                }

                return actualMonthText;
            }
        }


        public string ActualOperationsSum {
            get {
                decimal actualMoney = AllOperations.Sum(i => i.SignedCost);
                return actualMoney.ToString("C", Settings.getActualCultureInfo());
            }
        }

        private HashSet<int> visiblePayFormList;
        public HashSet<int> VisiblePayFormList {
            get {
                if (visiblePayFormList == null)
                    visiblePayFormList = new HashSet<int>(Dal.getAllMoneyAccounts().Select(i => i.Id));
                return visiblePayFormList;
            }
            set {
                if (!visiblePayFormList.Equals(value))
                    visiblePayFormList = value;
                SetNewOperationsList();
            }
        }

        private List<Operation> allOperations;
        private List<Operation> AllOperations {
            get {
                if (allOperations == null)
                    allOperations = setOperations();
                return allOperations;
            }
            set {
                allOperations = value;
            }
        }


        private void SetNewOperationsList() {
            AllOperations = setOperations();
            OnPropertyChanged("OperationsByDay");
            OnPropertyChanged("OperationsByCategory");
            OnPropertyChanged("ActualOperationsSum");
        }


        private List<Operation> setOperations() {
            return IsFuture ? Dal.getAllFutureOperations(VisiblePayFormList) : Dal.getAllOperations(ActualMonth, VisiblePayFormList);
        }


        public bool IsFuture {
            get {
                return ActualMonth > Date.FirstDayInMonth(DateTime.Today);
            }
        }

        private ObservableCollection<GroupInfoList<Operation>> operationsByDay;
        public ObservableCollection<GroupInfoList<Operation>> OperationsByDay {
            get {
                if (operationsByDay == null || monthOfDayGrouping != ActualMonth  || !visiblePayFormList.SetEquals(visiblePayFormListOfDayGrouping) ) {
                    operationsByDay = new ObservableCollection<GroupInfoList<Operation>>();
                    monthOfDayGrouping = ActualMonth;
                    visiblePayFormListOfDayGrouping = new HashSet<int>(visiblePayFormList);

                    var query = from item in AllOperations
                                group item by item.Date into g
                                orderby g.Key descending
                                select new {
                                    GroupName = g.Key,
                                    Items = g.OrderByDescending(i => i.Id)
                                };

                    foreach (var g in query) {
                        GroupInfoList<Operation> info = new GroupInfoList<Operation>() {
                            Key = new GroupHeaderByDay(g.GroupName),
                        };

                        foreach (var item in g.Items)
                            info.Add(item);

                        operationsByDay.Add(info);
                    }
                }

                return operationsByDay;
            }
        }
        

        public void RemoveOperation(Operation operation) {
            AllOperations.Remove(operation);

            if (operationsByDay != null) {
                GroupInfoList<Operation> group = operationsByDay.SingleOrDefault(i => i.Key.ToString() == operation.Date);
                group.Remove(operation);
                if (group.Count == 0)
                    operationsByDay.Remove(group);
            }

            if (OperationsByCategory != null) {
                try {
                    GroupInfoList<Operation> group = OperationsByCategory.SingleOrDefault(i => i.Key.ToString() == operation.CategoryId.ToString());
                    group.Remove(operation);
                    if (group.Count == 0)
                        OperationsByCategory.Remove(group);
                }
                catch { }
            }

            OnPropertyChanged("ActualOperationsSum");
        }


        public void AddOperation(Operation operation) {
            AllOperations.Add(operation);

            if (operationsByDay != null) {
                if (ActualMonth > Date.FirstDayInMonth(DateTime.Today)) {
                    if (string.IsNullOrEmpty(operation.Date)) {
                        GroupInfoList<Operation> group = operationsByDay.SingleOrDefault(i => string.IsNullOrEmpty(i.Key.ToString()));
                        if (group != null)
                            group.Insert(0, operation);
                        else {
                            group = new GroupInfoList<Operation> {
                                Key = new GroupHeaderByDay(string.Empty),
                            };
                            group.Add(operation);
                            operationsByDay.Insert(0, group);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(operation.Date) && ActualMonth.ToString("yyyy.MM") == operation.Date.Substring(0, 7)) {
                    GroupInfoList<Operation> group = operationsByDay.SingleOrDefault(i => i.Key.ToString() == operation.Date);
                    if (group != null) {
                        group.Insert(0, operation);
                    }
                    else {
                        group = new GroupInfoList<Operation> {
                            Key = new GroupHeaderByDay(operation.Date),
                        };
                        group.Add(operation);

                        int i;
                        for (i = 0; i < operationsByDay.Count; i++)
                            if (((GroupHeaderByDay)operationsByDay.ElementAt(i).Key).date.CompareTo(operation.Date) < 0)
                                break;

                        operationsByDay.Insert(i, group);
                    }
                }
            }

            if (OperationsByCategory != null) {
                try {
                    /// TO DO
                }
                catch { }
            }

            OnPropertyChanged("ActualOperationsSum");
        }


        private ObservableCollection<GroupInfoList<Operation>> operationsByCategory;
        public ObservableCollection<GroupInfoList<Operation>> OperationsByCategory {
            get {
                if (operationsByCategory == null || monthOfCategoryGrouping != ActualMonth || !visiblePayFormList.SetEquals(visiblePayFormListOfCategoryGrouping)) {
                    operationsByCategory = new ObservableCollection<GroupInfoList<Operation>>();
                    monthOfCategoryGrouping = ActualMonth;
                    visiblePayFormListOfCategoryGrouping = new HashSet<int>(visiblePayFormList);
                    GroupInfoList<Operation> info;

                    var query = from item in AllOperations
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

                        foreach (var item in g.Items.OrderByDescending(i => i.Id))
                            info.Add(item);

                        operationsByCategory.Add(info);
                    }
                }
                return operationsByCategory;
            }
        }


        public OperationData() {
            SetNewOperationsList();
        }


        private int HowManyEmptyCells {
            get {
                int dayOfWeek = (int)(ActualMonth.DayOfWeek) - (int)Settings.getFirstDayOfWeek();
                if (dayOfWeek < 1)
                    dayOfWeek += 7;
                return dayOfWeek;
            }
        }


        List<HeaderItem> operationHeaders;
        public List<HeaderItem> OperationHeaders {
            get {
                if (operationHeaders == null || VisiblePayFormList != null) {
                    operationHeaders = new List<HeaderItem>();

                    int dayOfWeek = HowManyEmptyCells;
                    for (int i = 0; i < dayOfWeek; i++)
                        operationHeaders.Add(new HeaderItem() { Day = String.Empty, IsEnabled = false });

                    for (int i = 1; i <= DateTime.DaysInMonth(ActualMonth.Year, ActualMonth.Month); i++) {

                        if (this.OperationsByDay.Any(k => ((GroupHeaderByDay)k.Key).dayNum == i.ToString()))
                            operationHeaders.Add(new HeaderItem() { Day = i.ToString(), IsEnabled = true });
                        else
                            operationHeaders.Add(new HeaderItem() { Day = i.ToString(), IsEnabled = false });
                    }
                }

                return operationHeaders;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
