using SQLite.Net.Attributes;
using System.Collections.ObjectModel;

namespace Finanse.Models {

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
            subCategories.Insert(0, subCategory);
        }
    }
}
