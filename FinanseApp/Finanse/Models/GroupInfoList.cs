using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class GroupInfoList<T> : ObservableCollection<Operation> {
        public object Key { get; set; }
        private decimal _decimalCost;

        public string cost {
            get {
                return decimalCost.ToString("C", Settings.GetActualCurrency());
            }
        }

        public decimal decimalCost {
            get {
                _decimalCost = 0;

                foreach (var item in this.Items)
                    _decimalCost += item.isExpense ? -item.Cost : item.Cost;

                return _decimalCost;
            }
        }

        public new IEnumerator<Operation> GetEnumerator() {
            return base.GetEnumerator();
        }
    }

    class GroupHeaderByCategory {
        private string _name = "";
        public string name {
            get {
                if (_name.Equals("")) {
                    _name = categoryId == -1 ? "Nieprzyporządkowane" : Dal.GetOperationCategoryById(categoryId).Name;
                }
                return _name;
            }
        }
        public int categoryId { get; set; }
        public string icon { get; set; }
        public FontFamily iconStyle { get; set; }
        public string color { get; set; }
        public double opacity { get; set; }
    }
    class GroupHeaderByDay {
        public string date { get; set; }
        public string dayNum { get; set; }
        public string dayNum00 { get; set; }
        public string day { get; set; }
        public string month { get; set; }

        public GroupHeaderByDay(string date) {

            if (date.Equals("")) {
                this.date = date;
                dayNum00 = "#";
                dayNum = "";
                day = "Bez daty";
                month = "";
            }

            else {
                DateTime dt = Convert.ToDateTime(date);
                DateTime dtToday = DateTime.Today;

                this.date = date;
                month = String.Format("{0:MMMM yyyy}", dt);
                dayNum00 = String.Format("{0:dd}", dt);
                dayNum = dt.Day.ToString();
                if (dt.Month == dtToday.Month && dt.Year == dtToday.Year) {
                    if (dt.Day == dtToday.Day)
                        day = "dzisiaj";
                    else if (dt.Day == dtToday.Day - 1)
                        day = "wczoraj";
                    else
                        day = String.Format("{0:dddd}", dt);
                }
                else
                    day = String.Format("{0:dddd}", dt);
            }
        }
    }
}