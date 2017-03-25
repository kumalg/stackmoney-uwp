using Finanse.DataAccessLayer;
using System;
using System.Collections.ObjectModel;

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
