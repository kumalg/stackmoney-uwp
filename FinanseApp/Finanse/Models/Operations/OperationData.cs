using Finanse.DataAccessLayer;
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

namespace Finanse.Models.Operations {
    public class OperationData : INotifyPropertyChanged {

        private DateTime monthOfDayGrouping;
        private DateTime monthOfCategoryGrouping;
        private HashSet<int> visiblePayFormListOfDayGrouping = new HashSet<int>();
        private HashSet<int> visiblePayFormListOfCategoryGrouping;
        private bool forceByDayUpdate = false;
        private bool forceByCategoryUpdate = false;


        private DateTime actualMonth = DateHelper.FirstDayInMonth(DateTime.Today);
        public DateTime ActualMonth {
            get {
                return actualMonth;
            }
            set {
                actualMonth = value;
                OnPropertyChanged("ActualMonthText");
                OnPropertyChanged("ActualOperationsSum");
                SetNewOperationsList();
            }
        }


        public string ActualMonthText {
            get {
                string actualMonthText = string.Empty;
                if (!IsFuture) {
                    actualMonthText = DateTimeFormatInfo.CurrentInfo.GetMonthName(ActualMonth.Month).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(ActualMonth.Month).Substring(1);
                    if (ActualMonth.Year != DateTime.Today.Year)
                        actualMonthText += " " + ActualMonth.Year;
                }
                else {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    actualMonthText = loader.GetString("plannedString");
                }

                return actualMonthText;
            }
        }

        public void ForceUpdate() {
            forceByDayUpdate = true;
            forceByCategoryUpdate = true;
            SetNewOperationsList();
        }

        public string ActualOperationsSum => AllOperations.Sum(i => i.SignedCost).ToString("C", Settings.ActualCultureInfo);

        private HashSet<int> visiblePayFormList;
        public HashSet<int> VisiblePayFormList {
            get {
                if (visiblePayFormList == null)
                    visiblePayFormList = new HashSet<int>(AccountsDal.GetAllMoneyAccounts().Select(i => i.Id));
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
                return allOperations ?? (allOperations = SetOperations());
            }
            set {
                allOperations = value;
            }
        }


        private void SetNewOperationsList() {
            AllOperations = SetOperations();
            OnPropertyChanged("OperationsByDay");
            OnPropertyChanged("OperationsByCategory");
            OnPropertyChanged("ActualOperationsSum");
        }


        private List<Operation> SetOperations() {
            return IsFuture ? Dal.GetAllFutureOperations(VisiblePayFormList) : Dal.GetAllOperations(ActualMonth, VisiblePayFormList);
        }


        public bool IsFuture => ActualMonth > DateHelper.FirstDayInMonth(DateTime.Today);

        private ObservableCollection<GroupInfoList<Operation>> operationsByDay;
        public ObservableCollection<GroupInfoList<Operation>> OperationsByDay {
            get {
                if (operationsByDay == null || monthOfDayGrouping != ActualMonth  || !visiblePayFormList.SetEquals(visiblePayFormListOfDayGrouping) || forceByDayUpdate) {
                    forceByDayUpdate = false;
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
                if (group != null) {
                    if (group.Count == 1)
                        operationsByDay.Remove(group);
                    else {
                        group.Remove(operation);
                    }
                }
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
                if (ActualMonth > DateHelper.FirstDayInMonth(DateTime.Today)) {
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
                            if (((GroupHeaderByDay)operationsByDay.ElementAt(i).Key).Date.CompareTo(operation.Date) < 0)
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
                if (operationsByCategory == null || monthOfCategoryGrouping != ActualMonth || !visiblePayFormList.SetEquals(visiblePayFormListOfCategoryGrouping) || forceByCategoryUpdate) {
                    forceByCategoryUpdate = false;
                    operationsByCategory = new ObservableCollection<GroupInfoList<Operation>>();
                    monthOfCategoryGrouping = ActualMonth;
                    visiblePayFormListOfCategoryGrouping = new HashSet<int>(visiblePayFormList);

                    var query = from item in AllOperations
                                group item by item.CategoryId into g
                                orderby g.Key descending
                                select new {
                                    GroupName = g.Key,
                                    Items = g
                                };

                    foreach (var g in query) {
                        /*
                        var info = new GroupInfoList<Operation> {
                            Key = new GroupHeaderByCategory {
                                //name = "Nieprzyporządkowane", /// WAŻNE MAXXXXXXXXXXXXXX
                                CategoryId = -1,
                                Icon = ((FontIcon)Application.Current.Resources["DefaultEllipseIcon"]).Glyph,
                                Color =
                                    ((SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"]).Color
                                    .ToString(),
                                Opacity = 0.2,
                            }
                        };


                        foreach (var item in Dal.GetAllCategories()) {
                            if (item.Id != g.GroupName)
                                continue;

                            ((GroupHeaderByCategory)info.Key).CategoryId = item.Id;
                            ((GroupHeaderByCategory)info.Key).Icon = item.Icon.Glyph;
                            ((GroupHeaderByCategory)info.Key).Color = item.Brush.ToString(); /// cymczasowe
                            ((GroupHeaderByCategory)info.Key).Opacity = 1;
                            break;
                        }

                        ((GroupHeaderByCategory)info.Key).IconStyle = new FontFamily(Settings.ActualIconStyle);
                        */

                        var info = new GroupInfoList<Operation> {
                            Key = new GroupHeaderByCategory(g.GroupName)
                        };

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
                int dayOfWeek = (int)(ActualMonth.DayOfWeek) - (int)Settings.FirstDayOfWeek;
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
                        operationHeaders.Add(new HeaderItem() { Day = string.Empty, IsEnabled = false });

                    for (int i = 1; i <= DateTime.DaysInMonth(ActualMonth.Year, ActualMonth.Month); i++) {

                        operationHeaders.Add(
                            OperationsByDay.Any(k => ((GroupHeaderByDay)k.Key).DayNum == i.ToString())
                                ? new HeaderItem() { Day = i.ToString(), IsEnabled = true }
                                : new HeaderItem() { Day = i.ToString(), IsEnabled = false });
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
