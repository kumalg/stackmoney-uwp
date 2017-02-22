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

    public sealed partial class OperationPatternsPage : Page {

        private ObservableCollection<OperationPattern> OperationPatterns = new ObservableCollection<OperationPattern>(Dal.getAllPatterns());

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
            var ContentDialogItem = new AcceptContentDialog("Czy chcesz usunąć szablon?");
            var result = await ContentDialogItem.ShowAsync();

            if (ContentDialogItem.isAccepted()) {
                removeOperationPatternFromList((OperationPattern)datacontext);
                Dal.deletePattern((OperationPattern)datacontext);
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new EditOperationContentDialog(((OperationPattern)datacontext));
            ContentDialogItem.PrimaryButtonClick += delegate {
                OperationPattern operationPattern = ContentDialogItem.editedOperationPattern();
                OperationPatterns[OperationPatterns
                    .IndexOf(OperationPatterns
                    .FirstOrDefault(i => i.Id == operationPattern.Id))] = operationPattern;
            };

            var result = await ContentDialogItem.ShowAsync();
        }

        public void removeOperationPatternFromList(OperationPattern operationPattern) {
            OperationPatterns.Remove(operationPattern);
        }

        public void addOperationPatternToList(OperationPattern operationPattern) {
            OperationPatterns.Insert(0, operationPattern);
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new OperationDetailsContentDialog(null, ((OperationPattern)datacontext).toOperation(), "pattern");

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void SzablonyListView_ItemClick(object sender, ItemClickEventArgs e) {
            OperationPattern thisOperation = (OperationPattern)e.ClickedItem;

            var ContentDialogItem = new OperationDetailsContentDialog(null, thisOperation.toOperation(), "pattern");

            var result = await ContentDialogItem.ShowAsync();
        }
    }
}
