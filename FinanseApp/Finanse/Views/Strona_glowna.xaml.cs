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

        private readonly ObservableCollection<GroupInfoList<Operation>> _source;
        Settings settings = Dal.GetSettings();

        bool isSelectionChanged = false;

        decimal actualMoney;

        public Strona_glowna() {

            this.InitializeComponent();

            Dal.CreateDB();

            _source = (new StoreData()).GetGroupsByDay();
            ContactsCVS.Source = _source;

            foreach (var group in _source) {
                actualMoney += group.decimalCost;
            }
            ActualMoneyBar.Text = actualMoney.ToString("C", new CultureInfo(settings.CultureInfoName));

            CategorizedCVS.Source = Operation.GetOperationsByCategoryGrouped();

        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e) {

            var ContentDialogItem = new NewOperationContentDialog(_source, null, "");

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

            var ContentDialogItem = new NewOperationContentDialog(_source, (Operation)datacontext, "edit");

            var result = await ContentDialogItem.ShowAsync();
            //this datacontext is probably some object of some type T
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new Delete_ContentDialog(_source, (Operation)datacontext,"");

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
    }
}
