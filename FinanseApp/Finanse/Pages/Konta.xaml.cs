using Finanse.DataAccessLayer;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Finanse.Pages {

    public sealed partial class Konta : Page {

        private ObservableCollection<MoneyAccount> MoneyAccounts = new ObservableCollection<MoneyAccount>(Dal.GetAllMoneyAccounts());

        public Konta() {

            this.InitializeComponent();
        }
    }
}
