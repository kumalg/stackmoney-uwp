using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Elements {
    public class CategoryGroupInfoList : List<object> {
        public object Key { get; set; }
        public string cost { get; set; }
        public string icon { get; set; }
        public string color { get; set; }
    }
}