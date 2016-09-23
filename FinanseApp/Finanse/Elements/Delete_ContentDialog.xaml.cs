using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Elements {
    public sealed partial class Delete_ContentDialog : ContentDialog {

        Operation operation;
        string whichOption;
        public Delete_ContentDialog(Operation operation, string whichOption) {

            this.InitializeComponent();

            this.operation = operation;
            this.whichOption = whichOption;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            switch (whichOption) {
                case "pattern": {
                        //Operations.Remove(operation);

                        Dal.DeletePattern(new OperationPattern {
                            Title = operation.Title,
                            Cost = operation.Cost,
                            CategoryId = operation.CategoryId,
                            SubCategoryId = operation.SubCategoryId,
                            Id = operation.Id,
                            isExpense = operation.isExpense,
                            MoreInfo = operation.MoreInfo,
                            MoneyAccountId = operation.MoneyAccountId
                        });

                        break;
                    }
                default: {
                        //Operations.Remove(operation);
                        Dal.DeleteOperation(operation);
                        break;
                    }
            }
        }
    }
}
