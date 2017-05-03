using System;

namespace Finanse.Models.Helpers {
    public class DateTimeHelper {
        public static string DateTimeUtcNowString => DateTime.UtcNow.ToString("yyyy.MM.dd HH:mm:ss");
    }
}
