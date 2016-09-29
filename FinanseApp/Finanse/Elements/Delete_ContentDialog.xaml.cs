using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        ObservableCollection<GroupInfoList<Operation>> _source;
        public Delete_ContentDialog(ObservableCollection<GroupInfoList<Operation>> _source, Operation operation, string whichOption) {

            this.InitializeComponent();

            this._source = _source;
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
                        GroupInfoList<Operation> group = _source.SingleOrDefault(i => i.Key.ToString() == operation.Date);
                        if (group.Count == 1)
                            _source.Remove(group);
                        else {
                            if (operation.isExpense)
                                group.decimalCost += operation.Cost;
                            else
                                group.decimalCost -= operation.Cost;

                            group.cost = group.decimalCost.ToString("C", Settings.GetActualCurrency());
                            group.Remove(operation);
                        }
                        //Operations.Remove(operation);
                        Dal.DeleteOperation(operation);
                        break;
                    }
            }
        }
    }
}
