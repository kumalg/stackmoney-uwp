using System.Collections.Generic;
using System.Linq;
using Finanse.DataAccessLayer;
using Finanse.Models.Operations;

namespace Finanse.Models.Extensions {
    public static class ListExtensions {

        public static List<Operation> LinkCategories(this List<Operation> operations) {
            var categoriesAndSubCategories = CategoriesDal.GetAllCategoriesAndSubCategories();
            operations.ForEach(op => op.GeneralCategory = categoriesAndSubCategories.FirstOrDefault(cat => cat.GlobalId == op.CategoryGlobalId));
            return operations;
        }
    }
}
