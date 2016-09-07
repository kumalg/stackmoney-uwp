using SQLite.Net.Attributes;

using System;
using System.Collections.Generic;
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
using Finanse.Elements;
using System.Collections.ObjectModel;
using SQLite.Net.Platform.WinRT;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {


    public sealed partial class Strona_glowna : Page {

        string path;
        SQLite.Net.SQLiteConnection conn;

        public ObservableCollection<Operation> Operations;

        public string DateToString(DateTimeOffset? Date) {
            string dateString;

            dateString = String.Format("{0:ddMMyyyy}", Date);

            return dateString;
        }

        public Strona_glowna() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            conn.CreateTable<Operation>();
            conn.CreateTable<OperationCategory>();
            conn.CreateTable<OperationSubCategory>();

            Operations = new ObservableCollection<Operation>(conn.Table<Operation>().OrderByDescending(o=>o.Date));

            var groups = from c in Operations
                         group c by DateToString(c.Date);
            //Set the grouped data to CollectionViewSource
            this.cvs.Source = groups;
        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e) {

            var ContentDialogItem = new NewOperationContentDialog(Operations, conn, null, -1, -1, null, -1, -1, null, null);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
             FrameworkElement senderElement = sender as FrameworkElement;
             FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
             flyoutBase.ShowAt(senderElement);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            Operation thisOperation = (Operation)datacontext;

            var ContentDialogItem = new NewOperationContentDialog(Operations, conn, thisOperation.Title, thisOperation.Id, thisOperation.Cost, 
                thisOperation.Date, thisOperation.CategoryId, thisOperation.SubCategoryId, thisOperation.ExpenseOrIncome, thisOperation.PayForm);

            var result = await ContentDialogItem.ShowAsync();
            //this datacontext is probably some object of some type T
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new Delete_ContentDialog(Operations, conn, (Operation)datacontext);

            var result = await ContentDialogItem.ShowAsync();

            //Operations.Remove((Operation)datacontext);
            //conn.Delete((Operation)datacontext);
            //this datacontext is probably some object of some type T
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void PivotItem_GotFocus(object sender, RoutedEventArgs e) {
        }
    }
}
