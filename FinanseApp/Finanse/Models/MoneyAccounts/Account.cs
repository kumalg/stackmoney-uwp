using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Finanse.Models.MoneyAccounts {
    public abstract class Account : ICloneable {

        private int id = -1;

        [PrimaryKey]
        public int Id {
            get {
                if (id == -1)
                    id = AccountsDal.GetHighestIdOfAccounts() + 1;
                return id;
            }
            set {
                id = value;
            }
        }
        public string Name { get; set; }
        public string ColorKey { get; set; }

        public SolidColorBrush SolidColorBrush {
            get {
                return string.IsNullOrEmpty(ColorKey) ?
                    (SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"] :
                    (SolidColorBrush)(((ResourceDictionary)Application.Current.Resources["ColorBase"]).FirstOrDefault(i => i.Key.Equals(ColorKey)).Value);
            }
        }

        public abstract string ActualMoneyValue { get; }

        public override string ToString() {
            return Name;
        }

        public Account Clone() {
            return (Account)MemberwiseClone();
        }

        public override int GetHashCode() {
            return Name.GetHashCode() * Id;
        }
        public override bool Equals(object o) {
            if (!(o is Account))
                return false;

            Account secondAccount = (Account)o;

            return
                secondAccount.Id == Id &&
                secondAccount.Name == Name &&
                secondAccount.ColorKey == ColorKey;
        }
    }
}
