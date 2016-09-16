using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {

    public class Settings {
        public string CultureInfoName {
            get; set;
        }

        public List<CultureInfo> GetCurrencyValues() {
            List<CultureInfo> lista = new List<CultureInfo>{

                new CultureInfo("en-US"),
                new CultureInfo("en-GB"),
                new CultureInfo("fr-FR"),
                new CultureInfo("ja-JP"),
                new CultureInfo("pl-PL"),
            };

            return lista;
        }

        public CultureInfo StringToCultureInfo(string s) {

            return new CultureInfo(s);
        }
    }
}
