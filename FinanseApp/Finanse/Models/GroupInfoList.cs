using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class GroupInfoList<T> : ObservableCollection<T> {
        public string Key { get; set; }
        public string dayNum { get; set; }
        public string day { get; set; }
        public string month { get; set; }
        public string cost { get; set; }
        public decimal decimalCost { get; set; }

        public new IEnumerator<T> GetEnumerator() {
            return base.GetEnumerator();
        }
    }

    class GroupHeaderByCategory {
        public OperationCategory category { get; set; }
        public string cost { get; set; }
        public double opacity { get; set; }
    }
    class GroupHeaderByDay {
        public string dayNum { get; set; }
        public string day { get; set; }
        public string month { get; set; }
        public string cost { get; set; }
        public decimal decimalCost { get; set; }
    }
}