using System.Collections.ObjectModel;
using System.Linq;

namespace Finanse.Models.MAccounts {
    public class MAccountWithSubMAccounts {

        public MAccount MAccount { get; set; }
        public ObservableCollection<SubMAccount> SubMAccounts { get; set; } = new ObservableCollection<SubMAccount>();


        public override string ToString() {
            var name = base.ToString();
            return SubMAccounts.Aggregate(name, (current, item) => current + ( "\n    " + item.Name ));
        }
    }
}