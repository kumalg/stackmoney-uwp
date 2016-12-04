using System;

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
    }
}
