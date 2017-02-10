using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.Statistics {
    public class SubCategoriesList {
        private List<ChartPart> list = new List<ChartPart>();
        public List<ChartPart> List {
            get {
                return list;
            }
            set {
                list = value;
            }
        }
        public OperationCategory Category {
            get; set;
        }

        public double Sum {
            get {
                return List.Sum(i => i.UnrelativeValue);
            }
        }
    }
}
