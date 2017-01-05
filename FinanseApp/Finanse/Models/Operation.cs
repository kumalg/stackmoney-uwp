namespace Finanse.Models {
    public class HeaderItem {
        public string Day { get; set; }
        public bool IsEnabled { get; set; }
    }
    public class Operation : OperationPattern {

        private string _date;
        public string Date {
            get {
                return _date;
            }
            set {
                if (_date != value)
                    _date = value;
            }
        }
        public override int GetHashCode() {
            return this.Title.GetHashCode() * this.Id;
        }
        public override bool Equals(object o) {
            if (o == null || !(o is Operation))
                return false;
            
            Operation secondOperation = (Operation)o;

            return 
                secondOperation.Id == Id &&
                secondOperation.Cost == Cost &&
                secondOperation.Title.Equals(Title) &&
                secondOperation.isExpense == isExpense &&
                secondOperation.Date == Date &&
                secondOperation.CategoryId == CategoryId &&
                secondOperation.SubCategoryId == SubCategoryId &&
                secondOperation.MoneyAccountId == MoneyAccountId &&
                secondOperation.MoreInfo.Equals(MoreInfo);
                
        }
    }
}
