using Finanse.DataAccessLayer;
using Finanse.Elements;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Dialogs {

    public sealed partial class OperationPatternsContentDialog : ContentDialog {

        public List<Operation> OperationPatterns = new List<Operation>();

        public OperationPatternsContentDialog() {

            this.InitializeComponent();

            foreach (OperationPattern item in Dal.GetAllPatterns()) {
                OperationPatterns.Add(new Operation {
                    Title = item.Title,
                    Cost = item.Cost,
                    CategoryId = item.CategoryId,
                    SubCategoryId = item.SubCategoryId,
                    Id = item.Id,
                    isExpense = item.isExpense,
                    MoreInfo = item.MoreInfo,
                    MoneyAccountId = item.MoneyAccountId
                });
            }
        }

        private void OperationPatternsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Hide();
        }
    }
}
