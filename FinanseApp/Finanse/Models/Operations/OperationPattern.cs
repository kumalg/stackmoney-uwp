using Finanse.DataAccessLayer;
using Finanse.Models.Categories;
using Finanse.Models.MoneyAccounts;
using SQLite.Net.Attributes;

namespace Finanse.Models.Operations {

    public class OperationPattern : SyncProperties {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string MoreInfo { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public decimal Cost { get; set; }
        public bool isExpense { get; set; }
        public int MoneyAccountId { get; set; }

        
        public decimal SignedCost => isExpense ? -Cost : Cost;
        public string TitleOrCategoryName {
            get {
                if (!string.IsNullOrEmpty(Title))
                    return Title;
                if (SubCategory != null)
                    return SubCategory.Name;
                if (Category != null)
                    return Category.Name;
                return string.Empty;
            }
        }



        private Account _account;
        public Account Account => _account ?? ( _account = AccountsDal.GetAccountById(MoneyAccountId) );


        private Category _category;
        public Category Category => _category ?? ( _category = Dal.GetCategoryById(CategoryId) );


        private SubCategory _subCategory;
        public SubCategory SubCategory {
            get {
                if (_subCategory != null)
                    return _subCategory;
                if (SubCategoryId == 0)
                    return _subCategory = null;
                return _subCategory = Dal.GetSubCategoryById(SubCategoryId);
            }
        }


        private Category _categoryIcon;
        public Category CategoryIcon {
            get {
                if (_categoryIcon != null)
                    return _categoryIcon;

                if (SubCategory != null)
                    return _categoryIcon = SubCategory;
                if (Category != null)
                    return _categoryIcon = Category;

                return _categoryIcon = new Category(); //TODO
            }
        }

        

        public Operation ToOperation() {
            return new Operation {
                Title = Title,
                Cost = Cost,
                Date = "",
                CategoryId = CategoryId,
                SubCategoryId = SubCategoryId,
                MoneyAccountId = MoneyAccountId,
                MoreInfo = MoreInfo,
                isExpense = isExpense,
                Id = Id,
                RemoteId = RemoteId,
                DeviceId = DeviceId
            };
        }



        public override int GetHashCode() {
            return Title.GetHashCode() * Id;
        }
        public override bool Equals(object o) {
            if (!(o is OperationPattern))
                return false;

            OperationPattern secondOperation = (OperationPattern)o;

            return //TODO trzeba przebudować bo zmieniła się struktura
                secondOperation.Id == Id &&
                secondOperation.Cost == Cost &&
                secondOperation.Title.Equals(Title) &&
                secondOperation.isExpense == isExpense &&
                secondOperation.CategoryId == CategoryId &&
                secondOperation.SubCategoryId == SubCategoryId &&
                secondOperation.MoneyAccountId == MoneyAccountId &&
                secondOperation.MoreInfo == MoreInfo;
        }
    }
}
