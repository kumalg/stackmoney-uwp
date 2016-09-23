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
using System.Globalization;
using Windows.Graphics.Display;
using Windows.Foundation.Metadata;
using Finanse.DataAccessLayer;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {


    public sealed partial class Strona_glowna : Page {

        //public ObservableCollection<GroupInfoList> Opy;

        //string path;
        //SQLite.Net.SQLiteConnection conn;

        //public ObservableCollection<Operation> Operations;
        //ObservableCollection<OperationCategory> OperationCategories;

        public static ObservableCollection<GroupInfoList> _source;

        public ObservableCollection<GroupInfoList> Operations;
        public ObservableCollection<CategoryGroupInfoList> OperationsByCategory;

        bool isSelectionChanged = false;

        decimal actualMoney;

        public Strona_glowna() {

            this.InitializeComponent();

            //path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            //conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            Dal.CreateDB();
            Settings settings = Dal.GetSettings();

            foreach (var operation in Dal.GetAllPersons()) {
                if (operation.isExpense)
                    actualMoney -= operation.Cost;
                else
                    actualMoney += operation.Cost;
            }
            ActualMoneyBar.Text = actualMoney.ToString("C", new CultureInfo(settings.CultureInfoName));


            Operations = Operation.GetOperationsGrouped();
            OperationsByCategory = Operation.GetOperationsByCategoryGrouped();

            _source = Operation.GetOperationsGrouped();

            ContactsCVS.Source = _source;
            CategorizedCVS.Source = Operation.GetOperationsByCategoryGrouped();

        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e) {

            var ContentDialogItem = new NewOperationContentDialog(null, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
             FrameworkElement senderElement = sender as FrameworkElement;
             FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
             flyoutBase.ShowAt(senderElement);
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new OperationDetailsContentDialog((Operation)datacontext, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new NewOperationContentDialog((Operation)datacontext, "edit");

            var result = await ContentDialogItem.ShowAsync();
            //this datacontext is probably some object of some type T
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new Delete_ContentDialog((Operation)datacontext,"");

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

        private async void OperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListView listView = sender as ListView;
            
            if (listView.SelectedIndex != -1 && isSelectionChanged) {
                Operation thisOperation = (Operation)listView.SelectedItem;

                var ContentDialogItem = new OperationDetailsContentDialog(thisOperation, "");
                listView.SelectedIndex = -1;
                var result = await ContentDialogItem.ShowAsync();
            }
        }

        private void ListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            OperacjeListView.SelectedIndex = -1;
            isSelectionChanged = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            DisplayInformation.GetForCurrentView().OrientationChanged += MainPage_OrientationChanged;
        }

        private void MainPage_OrientationChanged(DisplayInformation info, object args) {

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) {

                if (info.CurrentOrientation == DisplayOrientations.Landscape || info.CurrentOrientation == DisplayOrientations.LandscapeFlipped) {
                    FakeHamburgerButton.Width = 0;
                    UpperCommandBar.Visibility = Visibility.Visible;
                    LowerCommandBar.Visibility = Visibility.Collapsed;
                }

                else {
                    UpperCommandBar.Visibility = Visibility.Collapsed;
                    LowerCommandBar.Visibility = Visibility.Visible;
                    FakeHamburgerButton.Width = 48;
                }
            }
        }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e) {

            Operation operation = new Operation {
                Title = "Test",
                CategoryId = 1,
                SubCategoryId = 1,
                Cost = (decimal)20.99,
                Date = DateTime.Today,
                Id = 0,
                isExpense = true,
                MoneyAccountId = 2,
                MoreInfo = "Yolo"
            };

            GroupInfoList group = _source.Single(i => i.Key.ToString() == String.Format("{0:yyyy/MM/dd}", ((DateTimeOffset)operation.Date).LocalDateTime));

            group.Add(operation);
        }
    }
}
