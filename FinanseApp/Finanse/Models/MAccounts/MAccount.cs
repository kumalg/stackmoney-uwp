using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Models.MAccounts {
    public class MAccount : SyncProperties, ICloneable {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorKey { get; set; }



        public string ActualMoneyValue => MAccountsDal.AccountWithSubAccountsBalanceByGlobalId(GlobalId).ToString("C", Settings.ActualCultureInfo);

        public SolidColorBrush Brush => string.IsNullOrEmpty(ColorKey)
                    ? (SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"]
                    : (SolidColorBrush)( (ResourceDictionary)Application.Current.Resources["ColorBase"] ).FirstOrDefault(i => i.Key.Equals(ColorKey)).Value;



        public MAccount Clone() => (MAccount)MemberwiseClone();

        public override string ToString() => Name;

        public override int GetHashCode() => Name.GetHashCode() * Id;

        public override bool Equals(object o) {
            if (!( o is MAccount ))
                return false;

            var secondAccount = (MAccount)o;

            return
                //secondAccount.GlobalId == GlobalId &&
                secondAccount.Name == Name &&
                secondAccount.ColorKey == ColorKey;
        }
    }
}
