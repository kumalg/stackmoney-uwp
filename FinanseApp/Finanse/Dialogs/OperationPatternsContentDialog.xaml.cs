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

        private List<Operation> OperationPatterns = new List<Operation>();
        private Operation selectedOperation = new Operation();

        public OperationPatternsContentDialog(Operation selectedOperation) {

            this.InitializeComponent();
            //this.selectedOperation = selectedOperation;

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

            selectedOperation = OperationPatterns[0];
        }

        private void OperationPatternsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            selectedOperation = OperationPatterns[0]; // (Operation)((ListView)sender).SelectedItem;
            Hide();
        }

        public Operation setOperation () {

            return (Operation)OperationPatternsListView.SelectedItem;
        }
    }
}
