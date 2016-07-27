using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {

    public class OperationCategory {

        public string Name;
        public string Color;//kolor #
        public string Icon;//ikona
        public bool VisibleInIncomes;
        public bool VisibleInExpenses;

        public ObservableCollection<OperationSubCategory> subCategories = new ObservableCollection<OperationSubCategory>();

        public void addSubCategory(OperationSubCategory subCategory) {
            subCategories.Add(subCategory);
        }

        //pierdolondo
        public bool IsSubcategory;
        public string BossCategory;
    }
}
