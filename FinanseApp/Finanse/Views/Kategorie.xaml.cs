using Finanse.Elements;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class Kategorie : Page {

        string path;
        SQLite.Net.SQLiteConnection conn;

        public ObservableCollection<OperationCategory> OperationCategories = new ObservableCollection<OperationCategory>();
        public ObservableCollection<OperationCategory> OperationSubCategories = new ObservableCollection<OperationCategory>();

        public OperationCategory operationCategoryItem;
        public OperationSubCategory operationSubCategoryItem;

        public Kategorie() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            foreach (var message in conn.Table<OperationCategory>().OrderBy(category => category.Name)) {
                operationCategoryItem = message;

                foreach (var submessage in conn.Table<OperationSubCategory>().OrderBy(subCategory => subCategory.Name)) {
                    if (submessage.BossCategoryId == message.Id) {
                        operationCategoryItem.addSubCategory(submessage);
                    }
                }
                OperationCategories.Add(operationCategoryItem);
            }
        }

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, OperationSubCategories, conn);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {

        }

        private void EditCat_Click(object sender, RoutedEventArgs e) {

        }

        private void EditSubCat_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            OperationCategories.Remove((OperationCategory)datacontext);
            conn.Delete((OperationCategory)datacontext);
        }

        private void ExpandPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void DeleteSubCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            OperationSubCategory subCatItem= (OperationSubCategory)datacontext;
            OperationCategory BossCatItem = OperationCategories.Single(c => c.Id == subCatItem.BossCategoryId);

            BossCatItem.subCategories.Remove(subCatItem);

            conn.Delete((OperationSubCategory)datacontext);
        }
    }
}
