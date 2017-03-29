using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.Helpers {
    public class DateTimeOffsetHelper {
        public static string DateTimeOffsetNowString => DateTimeOffset.Now.ToString();
    }
}
