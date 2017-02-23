using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finanse.Models.Categories;

namespace Finanse.Models.Statistics {
    public class SubCategoriesList {
        public List<ChartPart> List { get; set; } = new List<ChartPart>();

        public Category Category {
            get; set;
        }

        public double Sum {
            get {
                return List.Sum(i => i.UnrelativeValue);
            }
        }
    }
}
