using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {

    public class OperationCategory {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Name  { get; set; } 
        public string Color { get; set; } 
        public string Icon { get; set; } 
        public bool VisibleInIncomes { get; set; } 
        public bool VisibleInExpenses { get; set; }

        public ObservableCollection<OperationSubCategory> subCategories = new ObservableCollection<OperationSubCategory>();

        public void addSubCategory(OperationSubCategory subCategory) {
            subCategories.Add(subCategory);
        }

        //pierdolondo
        public bool IsSubcategory { get; set; } 
        public string BossCategory { get; set; } 
    }
}
