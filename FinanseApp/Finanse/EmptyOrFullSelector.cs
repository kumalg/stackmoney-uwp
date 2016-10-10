using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Finanse
{
    public class EmptyOrFullSelector : DataTemplateSelector
    {
        public DataTemplate Full { get; set; }
        public DataTemplate Empty { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {

            bool isEmpty = true;

            if (item is HeaderItem) {
                var groupItem = item as HeaderItem;
                isEmpty = groupItem == null || !groupItem.IsEnabled;
            }

            // Disable empty items
            var selectorItem = container as SelectorItem;
            if (selectorItem != null) {
                selectorItem.IsEnabled = !isEmpty;
            }

            return (!isEmpty) ? Full : Empty;
        }
    }
}
