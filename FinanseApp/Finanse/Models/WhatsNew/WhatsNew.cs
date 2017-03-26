using System.Collections.Generic;
using System.Text;

namespace Finanse.Models.WhatsNew {
    public class WhatsNew {
        public string AppVersion { get; set; }
        public List<string> Description { get; set; }

        public string FormattedAppVersion => "###" + AppVersion;

        public string FormattedDescription {
            get {
                var stringBuilder = new StringBuilder();
                foreach (var line in Description)
                    stringBuilder.AppendLine("* " + line);
                return stringBuilder.ToString();
            }
        }

        public string Formatted => "\n" + FormattedAppVersion + "\n" + FormattedDescription;
    }
}
