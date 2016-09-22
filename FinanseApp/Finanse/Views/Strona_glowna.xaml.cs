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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {


    public sealed partial class Strona_glowna : Page {

        public ObservableCollection<GroupInfoList> Opy;

        string path;
        SQLite.Net.SQLiteConnection conn;

        public ObservableCollection<Operation> Operations;
        ObservableCollection<OperationCategory> OperationCategories;

        bool isSelectionChanged = false;

        decimal actualMoney;

        public Strona_glowna() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            conn.CreateTable<MoneyAccount>();
            conn.CreateTable<Operation>();
            conn.CreateTable<OperationPattern>();
            conn.CreateTable<OperationCategory>();
            conn.CreateTable<OperationSubCategory>();
            conn.CreateTable<Settings>();

            if(!conn.Table<Settings>().Any())
                conn.Insert(new Settings {
                    CultureInfoName = "en-US"
                });

            if (!conn.Table<MoneyAccount>().Any()) {
                conn.Insert(new MoneyAccount {
                    Name = "Gotówka"
                });
                conn.Insert(new MoneyAccount {
                    Name = "Karta VISA"
                });
            }

            Settings settings = conn.Table<Settings>().ElementAt(0);

            Operations = new ObservableCollection<Operation>(conn.Table<Operation>().OrderByDescending(o => o.Date));
            Operations.Insert(0, new Operation {
                Title = "SSS",
                Date = DateTime.Today,
                CategoryId = 1,
                SubCategoryId = 1,
                Cost = (decimal)20.99,
                isExpense = true,
                Id = 300,
                MoneyAccountId = 1,
                MoreInfo = "dupa",
                PayForm = "Visa"
            });
            OperationCategories = new ObservableCollection<OperationCategory>(conn.Table<OperationCategory>().OrderBy(o => o.Name));

            foreach (var operation in Operations) {
                if (operation.isExpense)
                    actualMoney -= operation.Cost;
                else
                    actualMoney += operation.Cost;
            }
            ActualMoneyBar.Text = actualMoney.ToString("C", new CultureInfo(settings.CultureInfoName));
            /*
            var groups = from c in Operations
                         group c by DateToString(c.Date);
            //Set the grouped data to CollectionViewSource
            this.cvs.Source = groups;*/
            Opy = Operation.GetOperationsGrouped(conn, Operations);
            ContactsCVS.Source = Opy;
            //ContactsCVS.Source = Operation.GetOperationsGrouped(conn, Operations);
            CategorizedCVS.Source = Operation.GetOperationsByCategoryGrouped(conn, Operations, OperationCategories);
        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e) {

            var ContentDialogItem = new NewOperationContentDialog(conn, null, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
             FrameworkElement senderElement = sender as FrameworkElement;
             FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
             flyoutBase.ShowAt(senderElement);
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            Operation thisOperation = (Operation)datacontext;

            var ContentDialogItem = new OperationDetailsContentDialog(Operations, conn, thisOperation);

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            Operation thisOperation = (Operation)datacontext;

            var ContentDialogItem = new NewOperationContentDialog(conn, thisOperation, "edit");

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

        private async void OperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListView listView = sender as ListView;
            
            if (listView.SelectedIndex != -1 && isSelectionChanged) {
                Operation thisOperation = (Operation)listView.SelectedItem;

                var ContentDialogItem = new OperationDetailsContentDialog(Operations, conn, thisOperation);
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
            Operations.Insert(0, new Operation {
                Title = "SSS",
                Date = DateTime.Today,
                CategoryId = 1,
                SubCategoryId = 1,
                Cost = (decimal)20.99,
                isExpense = true,
                Id = 300,
                MoneyAccountId = 1,
                MoreInfo = "dupa",
                PayForm = "Visa"
            });

            Opy = Operation.GetOperationsGrouped(conn, Operations);

            ContactsCVS = new CollectionViewSource() { IsSourceGrouped = true, Source = Opy };
        }
    }
}
