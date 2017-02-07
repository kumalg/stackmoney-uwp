using SQLite.Net.Attributes;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class OperationSubCategory : OperationCategory {

        public int BossCategoryId { get; set; }

        public OperationSubCategory(OperationCategory category) {
            if (category is OperationSubCategory)
                BossCategoryId = ((OperationSubCategory)category).BossCategoryId;
            else
                BossCategoryId = -1;

            ColorKey = category.ColorKey;
            IconKey = category.IconKey;
            Id = category.Id;
            Name = category.Name;
            VisibleInExpenses = category.VisibleInExpenses;
            VisibleInIncomes = category.VisibleInIncomes;
        }

        public OperationSubCategory() {
        }

        public override int GetHashCode() {
            return Name.GetHashCode() * Id;
        }
        public override bool Equals(object o) {
            if (o == null || !(o is OperationSubCategory))
                return false;

            return
                base.Equals((OperationCategory)o) &&
                ((OperationSubCategory)o).BossCategoryId == BossCategoryId;
        }
    }
}
