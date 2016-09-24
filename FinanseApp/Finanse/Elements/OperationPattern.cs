using SQLite.Net.Attributes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Elements {

    public class OperationPattern {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; }
        private string _title = string.Empty;
        private string _moreInfo = string.Empty;
        public string Title {
            get {
                return _title;
            }

            set {
                if (_title != value) {
                    _title = value;
                   // OnPropertyChanged("Title");
                }
            }
        }
        private int _categoryId;
        public int CategoryId {
            get {
                return _categoryId;
            }

            set {
                if (_categoryId != value) {
                    _categoryId = value;
                }
                else
                    _categoryId = 1;
            }
        }
        public int SubCategoryId { get; set; } 
        public decimal Cost { get; set; } 
        public bool isExpense { get; set; } 
        public string PayForm { get; set; }
        public int MoneyAccountId { get; set; }
        public string MoreInfo {
            get {
                return _moreInfo;
            }

            set {
                if (_moreInfo != value) {
                    _moreInfo = value;
                }
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
