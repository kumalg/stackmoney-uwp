using SQLite.Net.Attributes;

namespace Finanse.Models {

    public class OperationPattern {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string MoreInfo { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public decimal Cost { get; set; }
        public bool isExpense { get; set; }
        public int MoneyAccountId { get; set;
        }

        public decimal SignedCost {
            get {
                return isExpense ? -Cost : Cost;
            }
        }

        public Operation toOperation() {
            return new Operation {
                Title = Title,
                Cost = Cost,
                Date = "",
                CategoryId = CategoryId,
                SubCategoryId = SubCategoryId,
                MoneyAccountId = MoneyAccountId,
                MoreInfo = MoreInfo,
                isExpense = isExpense,
                Id = Id
            };
        }

        public override int GetHashCode() {
            return Title.GetHashCode() * Id;
        }
        public override bool Equals(object o) {
            if (o == null || !(o is OperationPattern))
                return false;

            OperationPattern secondOperation = (OperationPattern)o;

            return
                secondOperation.Id == Id &&
                secondOperation.Cost == Cost &&
                secondOperation.Title.Equals(Title) &&
                secondOperation.isExpense == isExpense &&
                secondOperation.CategoryId == CategoryId &&
                secondOperation.SubCategoryId == SubCategoryId &&
                secondOperation.MoneyAccountId == MoneyAccountId &&
                secondOperation.MoreInfo.Equals(MoreInfo);
        }
    }
}
