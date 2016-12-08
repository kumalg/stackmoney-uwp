using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Finanse.Pages {

    public sealed partial class Kategorie : Page {

        private ObservableCollection<OperationCategory> OperationCategories = new ObservableCollection<OperationCategory>(Dal.GetAllCategories());

        public Kategorie() {
            this.InitializeComponent();
        }

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, new OperationCategory {Id = -1 }, -1);
            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void EditCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            OperationCategory thisCategory = (OperationCategory)datacontext;
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, thisCategory, -1);

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditSubCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            OperationSubCategory thisSubCategory = (OperationSubCategory)datacontext;

            OperationCategory thisCategory = new OperationCategory {
                Id = thisSubCategory.Id,
                Name = thisSubCategory.Name,
                Color = thisSubCategory.Color,
                Icon = thisSubCategory.Icon,
                VisibleInExpenses = thisSubCategory.VisibleInExpenses,
                VisibleInIncomes = thisSubCategory.VisibleInIncomes
            };

            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, thisCategory, thisSubCategory.BossCategoryId);
            var result = await ContentDialogItem.ShowAsync();
        }

        private async void DeleteCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            var ContentDialogItem = new DeleteCategory_ContentDialog(OperationCategories, (OperationCategory)datacontext, null);
            var result = await ContentDialogItem.ShowAsync();
        }

        private void ExpandPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void DeleteSubCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            var ContentDialogItem = new DeleteCategory_ContentDialog(OperationCategories, null, (OperationSubCategory)datacontext);
            var result = await ContentDialogItem.ShowAsync();
        }

        private async void AddSubCat_Click(object sender, RoutedEventArgs e) {
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, new OperationCategory { Id = -1 }, -1);
            var result = await ContentDialogItem.ShowAsync();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            OperationCategories.Clear();

            foreach (var operationCategory in Dal.GetAllCategories().Where(i => i.VisibleInExpenses))
                OperationCategories.Add(operationCategory);
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e) {
            OperationCategories.Clear();

            foreach (var operationCategory in Dal.GetAllCategories().Where(i => i.VisibleInIncomes))
                OperationCategories.Add(operationCategory);
        }
    }
}
