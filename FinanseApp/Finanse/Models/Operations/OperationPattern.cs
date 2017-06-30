using Finanse.DataAccessLayer;
using Finanse.Models.Categories;
using Finanse.Models.Interfaces;
using Finanse.Models.MAccounts;
using SQLite.Net.Attributes;
//using SQLiteNetExtensions.Attributes;

namespace Finanse.Models.Operations {

    public class OperationPattern : ISync {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string MoreInfo { get; set; }
        //[ForeignKey(typeof(Category))]
        public string CategoryGlobalId { get; set; }
        //[OneToOne]
        //public Category Category { get; set; }
        public decimal Cost { get; set; }
        public bool isExpense { get; set; }
        public string MoneyAccountId { get; set; }
        public string LastModifed { get; set; }
        public bool IsDeleted { get; set; }
        public string GlobalId { get; set; }


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


        public Operation ToOperation() {
            return new Operation {
                Title = Title,
                Cost = Cost,
                Date = "",
                CategoryGlobalId = CategoryGlobalId,
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

            var secondOperation = (OperationPattern)o;

            return //TODO trzeba przebudować bo zmieniła się struktura
                secondOperation.Id == Id &&
                secondOperation.Cost == Cost &&
                secondOperation.Title.Equals(Title) &&
                secondOperation.isExpense == isExpense &&
                secondOperation.CategoryGlobalId == CategoryGlobalId &&
                secondOperation.MoneyAccountId == MoneyAccountId &&
                secondOperation.MoreInfo == MoreInfo;
        }
    }
}
