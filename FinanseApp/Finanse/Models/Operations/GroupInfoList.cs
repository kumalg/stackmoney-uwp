using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Finanse.DataAccessLayer;
using Finanse.Models.Categories;

namespace Finanse.Models.Operations {
    public class GroupInfoList<T> : ObservableCollection<Operation> {
        public object Key { get; set; }

        public string Cost => DecimalCost.ToString("C", Settings.ActualCultureInfo);
        
        public decimal DecimalCost => this.Sum(i=>i.SignedCost);
        
        public GroupInfoList() {
            CollectionChanged += delegate {
                OnPropertyChanged("Cost");
            };
        }

        public GroupInfoList(IEnumerable<Operation> operations) : base(operations) {
            CollectionChanged += delegate {
                OnPropertyChanged("Cost");
            };
        }

        public new IEnumerator<Operation> GetEnumerator() {
            return base.GetEnumerator();
        }

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class GroupHeaderByCategory {
        public double Opacity { get; set; } = 1;

        public Category Category { get; }

        public GroupHeaderByCategory(string categoryGlobalId) {
            Category = Dal.GetCategoryByGlobalId(categoryGlobalId) ?? new Category {
                Name = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("uncategorized"),
                Id = -1,
                ColorKey = "12",
                IconKey = "FontIcon_19"
            };
        }

        public override string ToString() {
            return Category.Id.ToString();
        }
    }
    public class GroupHeaderByDay {
        public DateTime DateTime { get; }

        public string Date => DateTime.ToString("yyyy.MM.dd");

        public string DayNum => DateTime.Day.ToString();

        public string DayNum00 => DateTime.Day.ToString("00");

        public string Day {
            get {
                var actualDate = DateTime.Today.Date;
                return DateTime.Equals(DateTime.MinValue) ?
                    new Windows.ApplicationModel.Resources.ResourceLoader().GetString("no_date")
                    : actualDate.Equals(DateTime.Date) ? new Windows.ApplicationModel.Resources.ResourceLoader().GetString("today")
                    : actualDate.AddDays(-1).Equals(DateTime.Date) ? new Windows.ApplicationModel.Resources.ResourceLoader().GetString("yesterday")
                    : DateTime.ToString("dddd");
            }
        }

        public GroupHeaderByDay(string date) {
            DateTime = string.IsNullOrEmpty(date)
                ? DateTime.MinValue
                : DateTime.Parse(date);
        }

        public override string ToString() {
            return Date;
        }
    }
}