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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Dialogs {
    public sealed partial class DeleteCategory_ContentDialog : ContentDialog {
        ObservableCollection<OperationCategory> OperationCategories;
        ObservableCollection<OperationSubCategory> OperationSubCategories;
        SQLite.Net.SQLiteConnection conn;
        OperationCategory operationCategory;
        OperationSubCategory operationSubCategory;
        public DeleteCategory_ContentDialog(ObservableCollection<OperationCategory> OperationCategories, SQLite.Net.SQLiteConnection conn, OperationCategory operationCategory, OperationSubCategory operationSubCategory) {
            this.InitializeComponent();
            this.OperationCategories = OperationCategories;
            this.conn = conn;
            this.operationCategory = operationCategory;
            this.operationSubCategory = operationSubCategory;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            if (operationCategory != null) {

                OperationCategories.Remove(operationCategory);
                conn.Delete(operationCategory);
                //foreach (var subCat in conn.Table<OperationSubCategory>().Where(x => x.BossCategoryId == operationCategory.Id))
                    //conn.Delete(subCat);
                conn.Delete(conn.Table<OperationSubCategory>().Where(x => x.BossCategoryId == operationCategory.Id));
            }
            else if (operationSubCategory != null) {

                OperationSubCategory subCatItem = operationSubCategory;
                OperationCategory BossCatItem = OperationCategories.Single(c => c.Id == subCatItem.BossCategoryId);

                BossCatItem.subCategories.Remove(subCatItem);

                conn.Delete(operationSubCategory);
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        }
    }
}
