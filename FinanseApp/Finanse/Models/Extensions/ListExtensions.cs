using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finanse.DataAccessLayer;
using Finanse.Models.Operations;

namespace Finanse.Models.Extensions {
    public static class ListExtensions {

        public static async Task<List<Operation>> LinkCategories(this List<Operation> operations) {
            var categoriesAndSubCategories = await CategoriesDal.GetAllCategoriesAndSubCategoriesAsync();
            operations.ForEach(op => op.GeneralCategory = categoriesAndSubCategories.FirstOrDefault(cat => cat.GlobalId == op.CategoryGlobalId));
            return operations;
        }
    }
}
