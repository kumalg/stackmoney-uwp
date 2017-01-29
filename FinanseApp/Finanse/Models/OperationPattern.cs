using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Finanse.Models {

    public class OperationPattern {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(OperationCategory))]
        public string Title { get; set; }
        public string MoreInfo { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public decimal Cost { get; set; }
        public bool isExpense { get; set; }
        public int MoneyAccountId { get; set; }

        public Operation toOperation() {
            return new Operation {
                Title = this.Title,
                Cost = this.Cost,
                Date = "",
                CategoryId = this.CategoryId,
                SubCategoryId = this.SubCategoryId,
                MoneyAccountId = this.MoneyAccountId,
                MoreInfo = this.MoreInfo,
                isExpense = this.isExpense,
                Id = this.Id
            };
        }

        public override int GetHashCode() {
            return this.Title.GetHashCode() * this.Id;
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
        /*
         * public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

protected virtual void OnPropertyChanged(string propertyName) {
    if (PropertyChanged != null) {
        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
*/

            /// <summary>
            /// może pokazywac błędnie jak edytujesz kategorię
            /// </summary>
/*
        private OperationCategory operationCategory;
        public OperationCategory OperationCategory {
            get {
                if (operationCategory != null) {

                }

                return operationCategory;
            }
        }
        */
    }
}
