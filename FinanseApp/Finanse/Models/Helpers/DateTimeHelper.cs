using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.Helpers {
    public class DateTimeHelper {
        public static string DateTimeUtcNowString => DateTime.UtcNow.ToString("yyyy.MM.dd HH:mm:ss");
    }
}
