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

namespace Finanse.Elements {
    public class CategoryGroupInfoList<T> : ObservableCollection<T> {
        public string Key { get; set; }
        public string cost { get; set; }
        public string icon { get; set; }
        public FontFamily iconStyle { get; set; }
        public double opacity { get; set; }
        public string color { get; set; }

        public new IEnumerator<T> GetEnumerator() {
            return base.GetEnumerator();
        }
    }
}