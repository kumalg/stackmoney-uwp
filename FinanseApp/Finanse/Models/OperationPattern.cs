using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Finanse.Models {

    public class OperationPattern {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        private string _title = string.Empty;
        private string _moreInfo = string.Empty;
        private int _categoryId;
        private int _subCategoryId;
        private decimal _cost;
        private bool _isExpense;
        private int _moneyAccountId;

        [ForeignKey(typeof(OperationCategory))]
        public string Title {
            get {
                return _title;
            }
            set {
                if (_title != value) {
                    _title = value; /* OnPropertyChanged("Title");*/
                }
            }
        }
        public string MoreInfo {
            get {
                return _moreInfo;
            }
            set {
                if (_moreInfo != value)
                    _moreInfo = value;
            }
        }
        public int CategoryId {
            get {
                return _categoryId;
            }

            set {
                if (_categoryId != value) {
                    _categoryId = value;
                }
            }
        }
        public int SubCategoryId {
            get {
                return _subCategoryId;
            }
            set {
                if (_subCategoryId != value)
                    _subCategoryId = value;
            }
        }
        public decimal Cost {
            get {
                return _cost;
            }
            set {
                if (_cost != value)
                    _cost = value;
            }
        }
        public bool isExpense {
            get {
                return _isExpense;
            }
            set {
                if (_isExpense != value)
                    _isExpense = value;
            }
        }
        public int MoneyAccountId {
            get {
                return _moneyAccountId;
            }
            set {
                if (_moneyAccountId != value)
                    _moneyAccountId = value;
            }
        }

        /*
         * public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

protected virtual void OnPropertyChanged(string propertyName) {
    if (PropertyChanged != null) {
        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
*/
    }
}
