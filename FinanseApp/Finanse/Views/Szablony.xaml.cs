using Finanse.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class Szablony : Page {

        public ObservableCollection<Operation> OperationPatterns = new ObservableCollection<Operation>();

        string path;
        SQLite.Net.SQLiteConnection conn;

        public Szablony() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

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

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new Delete_ContentDialog(OperationPatterns, conn, (Operation)datacontext,"pattern");

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new NewOperationContentDialog(conn, (Operation)datacontext, "editpattern");

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new OperationDetailsContentDialog(OperationPatterns, conn, (Operation)datacontext, "pattern");

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void SzablonyListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListView listView = sender as ListView;

            if (listView.SelectedIndex != -1) {
                Operation thisOperation = (Operation)listView.SelectedItem;

                var ContentDialogItem = new OperationDetailsContentDialog(OperationPatterns, conn, thisOperation, "pattern");
                listView.SelectedIndex = -1;
                var result = await ContentDialogItem.ShowAsync();
            }
        }
    }
}
