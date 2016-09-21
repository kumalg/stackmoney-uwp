using System;
using System.Collections.Generic;
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
using Finanse.Elements;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Globalization;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class OperationDetailsContentDialog : ContentDialog {

        public ObservableCollection<Operation> Operations;

        SQLite.Net.SQLiteConnection conn;

        Operation editedOperation;

        public OperationDetailsContentDialog(ObservableCollection<Operation> Operations, SQLite.Net.SQLiteConnection conn, Operation editedOperation) {

            this.InitializeComponent();

            IsPrimaryButtonEnabled = false;

            this.Operations = Operations;
            this.conn = conn;
            this.editedOperation = editedOperation;

            Settings settings = conn.Table<Settings>().ElementAt(0);

            NameValue.Text = editedOperation.Title;

            if (editedOperation.isExpense) {
                CostValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"] as SolidColorBrush;
                CostValueIcon.Glyph = "";
            }
            else {
                CostValue.Foreground = (SolidColorBrush)Application.Current.Resources["GreenColorStyle"] as SolidColorBrush;
                CostValueIcon.Glyph = "";
            }
            CostValue.Text = editedOperation.Cost.ToString("C", new CultureInfo(settings.CultureInfoName));

            DateValue.Text = String.Format("{0:dddd, dd MMMM yyyy}", ((DateTimeOffset)editedOperation.Date).LocalDateTime);

            if (conn.Table<OperationCategory>().Any(i => i.Id == editedOperation.CategoryId))
                CategoryValue.Text = conn.Table<OperationCategory>().Single(i => i.Id == editedOperation.CategoryId).Name;
            else
                CategoryValuePanel.Visibility = Visibility.Collapsed;

            if (conn.Table<OperationSubCategory>().Any(i => i.OperationCategoryId == editedOperation.SubCategoryId)) {
                SubCategoryValue.Text = conn.Table<OperationSubCategory>().Single(i => i.OperationCategoryId == editedOperation.SubCategoryId).Name;
            }
            else
                SubCategoryValuePanel.Visibility = Visibility.Collapsed;

            PayForm.Text = editedOperation.PayForm;

            if (editedOperation.MoreInfo != null) {
                MoreInfo.Text = editedOperation.MoreInfo;
            }
            else
                MoreInfoPanel.Visibility = Visibility.Collapsed;

            IsPrimaryButtonEnabled = true;
            IsSecondaryButtonEnabled = true;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            Hide();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {

            Hide();

            var ContentDialogItem = new NewOperationContentDialog(Operations, conn, editedOperation);

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {

            Hide();

            var ContentDialogItem = new Delete_ContentDialog(Operations, conn, editedOperation);

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            Hide();

            var ContentDialogItem = new NewOperationContentDialog(Operations, conn, editedOperation);

            var result = await ContentDialogItem.ShowAsync();
        }
        private async void DeleteButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            Hide();

            var ContentDialogItem = new Delete_ContentDialog(Operations, conn, editedOperation);

            var result = await ContentDialogItem.ShowAsync();
        }
    }
}
