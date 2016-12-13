using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using Finanse.Models;

namespace Finanse.Elements {

    public sealed partial class MoneyAccountTemplate : UserControl {

        private Models.MoneyAccount MoneyAccount {

            get {
                return this.DataContext as Models.MoneyAccount;
            }
        }

        public MoneyAccountTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
           
        }
    }
}
