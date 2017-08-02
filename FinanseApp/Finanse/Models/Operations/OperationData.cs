using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Finanse.Models.Extensions.DateTimeExtensions;

namespace Finanse.Models.Operations {
    public class OperationData : INotifyPropertyChanged {

        private DateTime _monthOfDayGrouping;
        private DateTime _monthOfCategoryGrouping;
        private HashSet<string> _visiblePayFormListOfDayGrouping = new HashSet<string>();
        private HashSet<string> _visiblePayFormListOfCategoryGrouping;
        private bool _forceByDayUpdate;
        private bool _forceByCategoryUpdate;


        private DateTime _actualMonth = DateTime.Today.First(); //DateHelper.First(DateTime.Today);
        public DateTime ActualMonth {
            get => _actualMonth;
            private set {
                _actualMonth = value;
                OnPropertyChanged(nameof(ActualMonthText));
                OnPropertyChanged(nameof(ActualOperationsSum));
                SetNewOperationsList();
            }
        }

        public void PrevMonth() {
            ActualMonth = ActualMonth.AddMonths(-1);
        }

        public void NextMonth() {
            if (ActualMonth > DateTime.Now.Date.First())
                return;
            ActualMonth = ActualMonth.AddMonths(1);
        }

        public bool SetMonth(DateTime newDate) {
            newDate = newDate.Date.First();

            if (ActualMonth == newDate)
                return false;

            var thisMonth = DateTime.Now.Date.First();
            var nextMonth = thisMonth.AddMonths(1).First();

            ActualMonth = newDate > thisMonth
                ? nextMonth
                : newDate;

            return true;
        }


        public string ActualMonthText {
            get {
                string actualMonthText;
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
            _forceByDayUpdate = true;
            _forceByCategoryUpdate = true;
            SetNewOperationsList();
        }

        public string ActualOperationsSum => AllOperations.Sum(i => i.SignedCost).ToString("C", Settings.ActualCultureInfo);

        private List<string> _visiblePayFormList;
        public List<string> VisiblePayFormList {
            get {
                return _visiblePayFormList ??
                       (_visiblePayFormList = new List<string>(MAccountsDal.GetAllAccountsAndSubAccounts().Select(i => i.GlobalId)));
            }
            set {
                if (!_visiblePayFormList.Equals(value))
                    _visiblePayFormList = value;
                SetNewOperationsList();
            }
        }

        private List<Operation> _allOperations;
        private List<Operation> AllOperations {
            get => _allOperations ?? (_allOperations = SetOperations());
            set => _allOperations = value;
        }


        private void SetNewOperationsList() {
            _allOperations?.Clear();
            AllOperations = SetOperations();
            OnPropertyChanged(nameof(OperationsByDay));
            OnPropertyChanged(nameof(OperationsByCategory));
            OnPropertyChanged(nameof(ActualOperationsSum));
        }


        private List<Operation> SetOperations() {
            return IsFuture
                ? Dal.GetAllFutureOperations(VisiblePayFormList)//.LinkCategories()
                : Dal.GetAllOperations(ActualMonth, VisiblePayFormList);//.LinkCategories();
        }


        public bool IsFuture => ActualMonth > DateTime.Today.First();

        private ObservableCollection<GroupInfoList<Operation>> _operationsByDay;
        public ObservableCollection<GroupInfoList<Operation>> OperationsByDay {
            get {
                if (_operationsByDay != null 
                    && _monthOfDayGrouping == ActualMonth 
                    && _visiblePayFormList.SequenceEqual(_visiblePayFormListOfDayGrouping) 
                    && !_forceByDayUpdate)
                    return _operationsByDay;

                _forceByDayUpdate = false;
                _monthOfDayGrouping = ActualMonth;
                _visiblePayFormListOfDayGrouping = new HashSet<string>(_visiblePayFormList);

                var query = AllOperations.GroupBy(item => item.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(g => new GroupInfoList<Operation>(g.OrderByDescending(i => i.LastModifed)) {
                        Key = new GroupHeaderByDay(g.Key),
                    });

                _operationsByDay?.Clear();
                return _operationsByDay = new ObservableCollection<GroupInfoList<Operation>>(query);
            }
        }
        

        public void RemoveOperation(Operation operation) {
            AllOperations.Remove(operation);

            if (_operationsByDay != null) {
                GroupInfoList<Operation> group = _operationsByDay.SingleOrDefault(i => i.Key.ToString() == operation.Date);
                if (group != null) {
                    if (group.Count == 1)
                        _operationsByDay.Remove(group);
                    else {
                        group.Remove(operation);
                    }
                }
            }

            if (OperationsByCategory != null) {
                try {
                    GroupInfoList<Operation> group =
                        OperationsByCategory.SingleOrDefault(i => i.Key.ToString() == operation.Category?.GlobalId.ToString());
                    group.Remove(operation);
                    if (group.Count == 0)
                        OperationsByCategory.Remove(group);
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                }
            }

            OnPropertyChanged(nameof(ActualOperationsSum));
        }


        public void AddOperation(Operation operation) {
            AllOperations.Add(operation);

            if (_operationsByDay != null) {
                if (ActualMonth > DateTime.Today.First()) {
                    if (string.IsNullOrEmpty(operation.Date)) {
                        GroupInfoList<Operation> group = _operationsByDay.SingleOrDefault(i => string.IsNullOrEmpty(i.Key.ToString()));
                        if (group != null)
                            group.Insert(0, operation);
                        else {
                            group = new GroupInfoList<Operation> {
                                Key = new GroupHeaderByDay(string.Empty),
                            };
                            group.Add(operation);
                            _operationsByDay.Insert(0, group);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(operation.Date) && ActualMonth.ToString("yyyy.MM") == operation.Date.Substring(0, 7)) {
                    GroupInfoList<Operation> group = _operationsByDay.SingleOrDefault(i => i.Key.ToString() == operation.Date);
                    if (group != null) {
                        group.Insert(0, operation);
                    }
                    else {
                        group = new GroupInfoList<Operation> {
                            Key = new GroupHeaderByDay(operation.Date),
                        };
                        group.Add(operation);

                        int i;
                        for (i = 0; i < _operationsByDay.Count; i++)
                            if (((GroupHeaderByDay)_operationsByDay.ElementAt(i).Key).Date.CompareTo(operation.Date) < 0)
                                break;

                        _operationsByDay.Insert(i, group);
                    }
                }
            }

            if (OperationsByCategory != null) {
                try {
                    //TODO
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                }
            }

            OnPropertyChanged(nameof(ActualOperationsSum));
        }


        private ObservableCollection<GroupInfoList<Operation>> _operationsByCategory;
        public ObservableCollection<GroupInfoList<Operation>> OperationsByCategory {
            get {
                if (_operationsByCategory != null
                    && _monthOfCategoryGrouping == ActualMonth
                    && _visiblePayFormList.SequenceEqual(_visiblePayFormListOfCategoryGrouping)
                    && !_forceByCategoryUpdate)
                    return _operationsByCategory;

                _forceByCategoryUpdate = false;
                _monthOfCategoryGrouping = ActualMonth;
                _visiblePayFormListOfCategoryGrouping = new HashSet<string>(_visiblePayFormList);

                var query = AllOperations.GroupBy(item => item.Category?.GlobalId)
                    .Select(g => new GroupInfoList<Operation>(g.OrderByDescending(i => i.LastModifed)) {
                        Key = new GroupHeaderByCategory(g.Key),
                    }).OrderBy(i => ((GroupHeaderByCategory)i.Key).Category.Name);

                _operationsByCategory?.Clear();
                return _operationsByCategory = new ObservableCollection<GroupInfoList<Operation>>(query);
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


        List<HeaderItem> _operationHeaders;
        public List<HeaderItem> OperationHeaders {
            get {
                if (_operationHeaders == null || VisiblePayFormList != null) {
                    _operationHeaders = new List<HeaderItem>();

                    int dayOfWeek = HowManyEmptyCells;
                    for (int i = 0; i < dayOfWeek; i++)
                        _operationHeaders.Add(new HeaderItem() { Day = string.Empty, IsEnabled = false });

                    for (int i = 1; i <= DateTime.DaysInMonth(ActualMonth.Year, ActualMonth.Month); i++) {

                        _operationHeaders.Add(
                            OperationsByDay.Any(k => ((GroupHeaderByDay)k.Key).DayNum == i.ToString())
                                ? new HeaderItem() { Day = i.ToString(), IsEnabled = true }
                                : new HeaderItem() { Day = i.ToString(), IsEnabled = false });
                    }
                }

                return _operationHeaders;
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
