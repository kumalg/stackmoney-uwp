using System.Collections.ObjectModel;

namespace Finanse.Models.Categories {
    public class CategoryWithSubCategories {

        public Category Category { get; set; }
        public ObservableCollection<SubCategory> SubCategories { get; set; } = new ObservableCollection<SubCategory>();


        public override int GetHashCode() {
            return Category.GetHashCode() * Category.Id;
        }
        public override bool Equals(object o) {
            if (!(o is CategoryWithSubCategories))
                return false;

            return Category.Equals(((CategoryWithSubCategories)o).Category);
        }
    }
}