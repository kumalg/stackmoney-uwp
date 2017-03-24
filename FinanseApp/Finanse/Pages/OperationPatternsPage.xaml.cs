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
using Finanse.Models.Operations;

namespace Finanse.Pages {

    public sealed partial class OperationPatternsPage : Page {

        private ObservableCollection<OperationPattern> OperationPatterns = new ObservableCollection<OperationPattern>(Dal.GetAllPatterns());

        public OperationPatternsPage() {
            this.InitializeComponent();
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
            var contentDialogItem = new AcceptContentDialog("Czy chcesz usunąć szablon?");
            var result = await contentDialogItem.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            RemoveOperationPatternFromList((OperationPattern)datacontext);
            Dal.DeletePattern((OperationPattern)datacontext);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var contentDialogItem = new EditOperationContentDialog(((OperationPattern)datacontext));
            contentDialogItem.PrimaryButtonClick += delegate {
                OperationPattern operationPattern = contentDialogItem.EditedOperationPattern();
                OperationPatterns[OperationPatterns
                    .IndexOf(OperationPatterns
                    .FirstOrDefault(i => i.Id == operationPattern.Id))] = operationPattern;
            };

            var result = await contentDialogItem.ShowAsync();
        }

        public void RemoveOperationPatternFromList(OperationPattern operationPattern) {
            OperationPatterns.Remove(operationPattern);
        }

        public void AddOperationPatternToList(OperationPattern operationPattern) {
            OperationPatterns.Insert(0, operationPattern);
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var contentDialogItem = new OperationDetailsContentDialog((OperationPattern)datacontext, "pattern");

            var result = await contentDialogItem.ShowAsync();
        }

        private async void SzablonyListView_ItemClick(object sender, ItemClickEventArgs e) {
            OperationPattern thisPattern = (OperationPattern)e.ClickedItem;

            var contentDialogItem = new OperationDetailsContentDialog(thisPattern, "pattern");

            var result = await contentDialogItem.ShowAsync();
        }
    }
}
