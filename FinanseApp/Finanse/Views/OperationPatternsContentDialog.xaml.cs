using Finanse.Elements;
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

namespace Finanse.Views {

    public sealed partial class OperationPatternsContentDialog : ContentDialog {

        public List<Operation> OperationPatterns = new List<Operation>();
        SQLite.Net.SQLiteConnection conn;

        public OperationPatternsContentDialog(SQLite.Net.SQLiteConnection conn) {

            this.InitializeComponent();
            this.conn = conn;

            foreach (OperationPattern item in conn.Table<OperationPattern>()) {
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

        private async void OperationPatternsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Hide();

            Operation thisOperation = (Operation)OperationPatternsListView.SelectedItem;

            var ContentDialogItem = new NewOperationContentDialog(conn, thisOperation, "pattern");

            var result = await ContentDialogItem.ShowAsync();
        }
    }
}
