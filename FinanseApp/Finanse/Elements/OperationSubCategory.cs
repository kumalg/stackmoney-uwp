using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Finanse.Elements {
    public class OperationSubCategory {

        [PrimaryKey, AutoIncrement]
        public int OperationCategoryId { get; set; } 
        public string Name  { get; set; } 
        public string Color { get; set; } 
        public string Icon { get; set; } 
        public int BossCategoryId { get; set; } 
        public bool VisibleInIncomes { get; set; } 
        public bool VisibleInExpenses { get; set; } 
    }
}
