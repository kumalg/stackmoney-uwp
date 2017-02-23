using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Finanse.Models.Statistics {

    public class CategoryChartPart : ChartPart {
        public ObservableCollection<ChartPart> ValuesBySubCategory { get; set; }

        public ObservableCollection<ChartPart> GetValuesGroupedBySubCategory() {
            DateTime minDate = new DateTime(2016, 1, 1);
            DateTime maxDate = DateTime.Today;
            return new ObservableCollection<ChartPart>(StatisticsDal.GetExpensesFromCategoryGroupedBySubCategoryInRange(minDate, maxDate, Tag));
        }
    }
}
