using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {
    public class OperationSubCategory {

        [PrimaryKey, AutoIncrement]
        public int OperationCategoryId { get; set; } 
        public string Name  { get; set; } 
        public string Color { get; set; } 
        public string Icon { get; set; } 
        public bool VisibleInIncomes { get; set; } 
        public bool VisibleInExpenses { get; set; } 
    }
}
