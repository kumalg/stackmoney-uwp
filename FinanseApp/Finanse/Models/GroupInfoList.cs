using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class GroupInfoList<T> : ObservableCollection<Operation> {
        public object Key { get; set; }

        public string Cost => DecimalCost.ToString("C", Settings.GetActualCultureInfo());


        public decimal DecimalCost {
            get {
                return this.Sum(i=>i.SignedCost);
            }
        }

        public new IEnumerator<Operation> GetEnumerator() {
            return base.GetEnumerator();
        }


        protected override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public GroupInfoList() {
            CollectionChanged += delegate {
                OnPropertyChanged("Cost");
            };
        }
    }

    class GroupHeaderByCategory {
        private string _name = "";
        public string Name {
            get {
                if (_name.Equals("")) {
                    _name = CategoryId == -1 ? "Nieprzyporządkowane" : Dal.GetCategoryById(CategoryId).Name;
                }
                return _name;
            }
        }
        public int CategoryId { get; set; }
        public string Icon { get; set; }
        public FontFamily IconStyle { get; set; }
        public string Color { get; set; }
        public double Opacity { get; set; }
        public override string ToString() {
            return CategoryId.ToString();
        }
    }
    class GroupHeaderByDay {
        public DateTime ActualMonth { get; set; }
        public string Date { get; set; }
        public string DayNum { get; set; }
        public string DayNum00 { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public override string ToString() {
            return Date;
        }

        public GroupHeaderByDay(string date) {

            if (date.Equals("")) {
                this.Date = date;
                DayNum00 = "#";
                DayNum = "";
                Day = "Bez daty";
                Month = "";
            }

            else {
                DateTime dt = Convert.ToDateTime(date);
                DateTime dtToday = DateTime.Today;

                this.Date = date;
                Month = String.Format("{0:MMMM yyyy}", dt);
                DayNum00 = String.Format("{0:dd}", dt);
                DayNum = dt.Day.ToString();
                if (dt.Month == dtToday.Month && dt.Year == dtToday.Year) {
                    if (dt.Day == dtToday.Day)
                        Day = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("today");
                    else if (dt.Day == dtToday.Day - 1)
                        Day = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("yesterday");
                    else
                        Day = String.Format("{0:dddd}", dt);
                }
                else
                    Day = String.Format("{0:dddd}", dt);
            }
        }
    }
}