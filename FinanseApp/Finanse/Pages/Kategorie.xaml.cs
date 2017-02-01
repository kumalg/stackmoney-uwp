using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Finanse.Pages {

    public sealed partial class Kategorie : Page, INotifyPropertyChanged {

        private ObservableCollection<OperationCategory> operationCategories;
        private ObservableCollection<OperationCategory> OperationCategories {
            get {
                return operationCategories;
            }
            set {
                operationCategories = value;
                RaisePropertyChanged("OperationCategories");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


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
                ColorKey = thisSubCategory.ColorKey,
                IconKey = thisSubCategory.IconKey,
                VisibleInExpenses = thisSubCategory.VisibleInExpenses,
                VisibleInIncomes = thisSubCategory.VisibleInIncomes
            }; /// po co to?????? /// a dobra... bo musze z subkategori zrobic kategorię

            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, thisCategory, thisSubCategory.BossCategoryId);
            var result = await ContentDialogItem.ShowAsync();
        }

        private async void DeleteCat_Click(object sender, RoutedEventArgs e) {
            OperationCategory operationCategory = (e.OriginalSource as FrameworkElement).DataContext as OperationCategory;

            AcceptContentDialog deleteContentDialog = new AcceptContentDialog("Czy chcesz usunąć kategorię?");

            var result = await deleteContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                OperationCategories.Remove(operationCategory);
                Dal.deleteCategoryWithSubCategories(operationCategory.Id);
            }
        }

        private async void DeleteSubCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            var ContentDialogItem = new DeleteCategory_ContentDialog(OperationCategories, null, (OperationSubCategory)datacontext);
            var result = await ContentDialogItem.ShowAsync();
        }

        private async void AddSubCat_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, (OperationCategory)datacontext);
            var result = await ContentDialogItem.ShowAsync();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {
            OperationCategories = new ObservableCollection<OperationCategory>(Dal.getOperationCategoriesWithSubCategoriesInExpenses());
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e) {
            OperationCategories = new ObservableCollection<OperationCategory>(Dal.getOperationCategoriesWithSubCategoriesInIncomes());
        }

        private void SubOperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListView listView = sender as ListView;
            listView.SelectedIndex = -1;
        }
        
        private void ExpandPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            showFlyoutBase(sender);
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e) {
            showFlyoutBase(sender);
        }

        private void showFlyoutBase(object sender) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }
    }
}
