using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {
    public class GroupInfoList : List<object> {
        public object Key {
            get; set;
        }
        public string cost{
            get; set;
        }

        public string keyToDayNumb(int i) {
            DateTime dt = Convert.ToDateTime(Key);
            return String.Format("{0:dd}", dt);
        }

        public string keyToDay(int i) {
            DateTime dt = Convert.ToDateTime(Key);
            return String.Format("{0:dddd}", dt);
        }

        public string keyToMonth(int i) {
            DateTime dt = Convert.ToDateTime(Key);
            return String.Format("{0:MMMM yyyy}", dt);
        }
    }
}