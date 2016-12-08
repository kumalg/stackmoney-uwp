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
        public string name { get; set; }
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
                dayNum00 = "#";
                dayNum = "";
                day = "Bez daty";
                month = "";
            }

            else {
                DateTime dt = Convert.ToDateTime(date);

                this.date = date;
                dayNum00 = String.Format("{0:dd}", dt);
                dayNum = dt.Day.ToString();
                day = String.Format("{0:dddd}", dt);
                month = String.Format("{0:MMMM yyyy}", dt);
            }
        }
    }
}