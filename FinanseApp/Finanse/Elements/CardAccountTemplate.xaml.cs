using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class CardAccountTemplate : UserControl {

        private CardAccount CardAccount => DataContext as CardAccount;

        public CardAccountTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}
