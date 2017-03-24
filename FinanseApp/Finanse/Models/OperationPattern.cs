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
        public int MoneyAccountId { get; set; }
        public string LastModifed { get; set; }
        public string DeviceId { get; set; } //ale prawdopodobie tylko get
        public int RemoteId { get; set; }
        public bool IsDeleted { get; set; }

        public decimal SignedCost => isExpense ? -Cost : Cost;
        
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

            return
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
