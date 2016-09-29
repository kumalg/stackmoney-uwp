using Finanse.DataAccessLayer;
using Finanse.Dialogs;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Pages {

    public sealed partial class Kategorie : Page {

        string path;
        SQLite.Net.SQLiteConnection conn;

        public ObservableCollection<OperationCategory> OperationCategories = new ObservableCollection<OperationCategory>();

        public OperationCategory operationCategoryItem;
        public OperationSubCategory operationSubCategoryItem;

        public Kategorie() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            foreach (var message in Dal.GetAllCategories()) {

                foreach (var submessage in Dal.GetOperationSubCategoryByBossId(message.Id)) {
                        message.addSubCategory(submessage);
                }

                OperationCategories.Add(message);
            }
        }

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, conn, new OperationCategory {Id = -1 }, -1);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {

        }

        private async void EditCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            OperationCategory thisCategory = (OperationCategory)datacontext;

            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, conn, thisCategory, -1);

            var result = await ContentDialogItem.ShowAsync();
            //this datacontext is probably some object of some type T
        }

        private async void EditSubCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            OperationSubCategory thisSubCategory = (OperationSubCategory)datacontext;
            OperationCategory thisCategory = new OperationCategory {
                Id = thisSubCategory.OperationCategoryId,
                Name = thisSubCategory.Name,
                Color = thisSubCategory.Color,
                Icon = thisSubCategory.Icon,
                VisibleInExpenses = thisSubCategory.VisibleInExpenses,
                VisibleInIncomes = thisSubCategory.VisibleInIncomes
            };

            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, conn, thisCategory, thisSubCategory.BossCategoryId);

            var result = await ContentDialogItem.ShowAsync();
            //this datacontext is probably some object of some type T
        }

        private async void DeleteCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new DeleteCategory_ContentDialog(OperationCategories, conn, (OperationCategory)datacontext, null);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void ExpandPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void DeleteSubCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new DeleteCategory_ContentDialog(OperationCategories, conn, null, (OperationSubCategory)datacontext);

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void AddSubCat_Click(object sender, RoutedEventArgs e) {
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, conn, new OperationCategory { Id = -1 }, -1);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Icon_OperationTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            TextBlock yco = sender as TextBlock;

            yco.FontFamily = new FontFamily(Settings.GetActualIconStyle());
        }
    }
}
