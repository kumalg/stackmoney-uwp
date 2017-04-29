using Finanse.DataAccessLayer;
using Finanse.Models.Categories;
using Finanse.Models.MAccounts;
using Finanse.Models.MoneyAccounts;
using SQLite.Net.Attributes;

namespace Finanse.Models.Operations {

    public class OperationPattern : SyncProperties {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string MoreInfo { get; set; }
        public string CategoryGlobalId { get; set; }
       // public string CategoryId { get; set; }
        //public string SubCategoryId { get; set; }
        public decimal Cost { get; set; }
        public bool isExpense { get; set; }
        public string MoneyAccountId { get; set; }

        
        public decimal SignedCost => isExpense ? -Cost : Cost;
        public string TitleOrCategoryName {
            get {
                if (!string.IsNullOrEmpty(Title))
                    return Title;
                if (GeneralCategory != null)
                    return GeneralCategory.Name;
                return string.Empty;
            }
        }



        private MAccount _account;
        public MAccount Account => _account ?? ( _account = MAccountsDal.GetAccountByGlobalId(MoneyAccountId) );



        private Category _generalCategory;

        [Ignore]
        public Category GeneralCategory {
            get => _generalCategory ?? (_generalCategory = CategoriesDal.GetCategoryByGlobalId(CategoryGlobalId));
            set => _generalCategory = value;
        }

        public Category Category => GeneralCategory is SubCategory
            ? ((SubCategory)GeneralCategory).ParentCategory
            : GeneralCategory;


        public SubCategory SubCategory => GeneralCategory as SubCategory;

            /*
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
        }*/

        public Operation ToOperation() {
            return new Operation {
                Title = Title,
                Cost = Cost,
                Date = "",
                CategoryGlobalId = CategoryGlobalId,
                //CategoryId = CategoryId,
                //SubCategoryId = SubCategoryId,
                MoneyAccountId = MoneyAccountId,
                MoreInfo = MoreInfo,
                isExpense = isExpense,
                Id = Id,
                GlobalId = GlobalId
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
                secondOperation.CategoryGlobalId == CategoryGlobalId &&
                //secondOperation.CategoryId == CategoryId &&
                //secondOperation.SubCategoryId == SubCategoryId &&
                secondOperation.MoneyAccountId == MoneyAccountId &&
                secondOperation.MoreInfo == MoreInfo;
        }
    }
}
