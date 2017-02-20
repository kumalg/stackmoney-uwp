using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class CardAccountTemplate : UserControl {

        private Models.MoneyAccounts.CardAccount CardAccount {

            get {
                return this.DataContext as CardAccount;
            }
        }

        public CardAccountTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
