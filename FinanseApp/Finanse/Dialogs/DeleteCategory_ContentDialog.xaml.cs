using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {
    public sealed partial class DeleteCategory_ContentDialog : ContentDialog {
        ObservableCollection<OperationCategory> OperationCategories;
        OperationCategory operationCategory;
        OperationSubCategory operationSubCategory;
        public DeleteCategory_ContentDialog(ObservableCollection<OperationCategory> OperationCategories, OperationCategory operationCategory, OperationSubCategory operationSubCategory) {
            this.InitializeComponent();
            this.OperationCategories = OperationCategories;
            this.operationCategory = operationCategory;
            this.operationSubCategory = operationSubCategory;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            if (operationCategory != null) {

                OperationCategories.Remove(operationCategory);
                Dal.deleteCategory(operationCategory);
            }
            else if (operationSubCategory != null) {

                OperationSubCategory subCatItem = operationSubCategory;
                OperationCategory BossCatItem = OperationCategories.Single(c => c.Id == subCatItem.BossCategoryId);

                BossCatItem.subCategories.Remove(subCatItem);

                Dal.deleteSubCategory(operationSubCategory);
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        }
    }
}
