using System.Collections.Generic;
using System.Linq;
using Finanse.Charts.Data;
using Finanse.Models.Categories;

namespace Finanse.Models.Statistics {
    public class SubCategoriesList {
        public List<ChartDataItem> List { get; set; } = new List<ChartDataItem>();

        public Category Category { get; set; }

        public double Sum => List.Sum(i => i.Value);
    }
}
