using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Finanse.Models;
using Finanse.Models.Helpers;
using Finanse.Models.Operations;

namespace Finanse.Pages {

    public sealed partial class OperationPatternsPage {

        private readonly ObservableCollection<OperationPattern> _operationPatterns = new ObservableCollection<OperationPattern>(Dal.GetAllPatterns());

        public OperationPatternsPage() {
            InitializeComponent();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) => Flyouts.ShowFlyoutBase(sender);
        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) => Flyouts.ShowFlyoutBase(sender);

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = ( e.OriginalSource as FrameworkElement )?.DataContext;
            ShowDeleteContentDialog((OperationPattern)datacontext);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = ( e.OriginalSource as FrameworkElement )?.DataContext;
            ShowEditContentDialog((OperationPattern)datacontext);
        }

        private async void ShowEditContentDialog(OperationPattern operationPattern) {
            var contentDialogItem = new EditOperationContentDialog(operationPattern);
            var result = await contentDialogItem.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            OperationPattern newOperationPattern = contentDialogItem.EditedOperationPattern();
            _operationPatterns[_operationPatterns.IndexOf(operationPattern)] = newOperationPattern;
        }

        private async void ShowDeleteContentDialog(OperationPattern operationPattern) {
            var contentDialogItem = new AcceptContentDialog("Czy chcesz usunąć szablon?");
            var result = await contentDialogItem.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            RemoveOperationPatternFromList(operationPattern);
            Dal.DeletePattern(operationPattern);
        }

        private async void ShowDetailsContentDialog(OperationPattern operationPattern) {
            var contentDialogItem = new OperationDetailsContentDialog(operationPattern, "pattern", Settings.Theme);
            var result = await contentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary)
                ShowEditContentDialog(operationPattern);
            else if (result == ContentDialogResult.Secondary)
                ShowDeleteContentDialog(operationPattern);
        }

        public void RemoveOperationPatternFromList(OperationPattern operationPattern) => _operationPatterns.Remove(operationPattern);

        public void AddOperationPatternToList(OperationPattern operationPattern) => _operationPatterns.Insert(0, operationPattern);

        private void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = ( e.OriginalSource as FrameworkElement )?.DataContext;
            ShowDetailsContentDialog((OperationPattern)datacontext);
        }

        private void SzablonyListView_ItemClick(object sender, ItemClickEventArgs e) {
            OperationPattern thisPattern = (OperationPattern)e.ClickedItem;
            ShowDetailsContentDialog(thisPattern);
        }
    }
}
