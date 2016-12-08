using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {
    public sealed partial class Delete_ContentDialog : ContentDialog {

        Operation operation;
        string whichOption;
        ObservableCollection<GroupInfoList<Operation>> groups;
        public Delete_ContentDialog(ObservableCollection<GroupInfoList<Operation>> groups, Operation operation, string whichOption) {

            this.InitializeComponent();

            this.groups = groups;
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
                        GroupInfoList<Operation> group = groups.SingleOrDefault(i => ((GroupHeaderByDay)i.Key).date == operation.Date);
                        if (group.Count == 1)
                            groups.Remove(group);
                        else {
                            //group.decimalCost += operation.isExpense ? operation.Cost : -operation.Cost;

                            //group.cost = group.decimalCost.ToString("C", Settings.GetActualCurrency());
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
