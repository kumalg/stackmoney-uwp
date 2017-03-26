using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            var contentDialogItem = new AcceptContentDialog("Czy chcesz usunąć szablon?");
            var result = await contentDialogItem.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            RemoveOperationPatternFromList((OperationPattern)datacontext);
            Dal.DeletePattern((OperationPattern)datacontext);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;

            var contentDialogItem = new EditOperationContentDialog(((OperationPattern)datacontext));
            contentDialogItem.PrimaryButtonClick += delegate {
                OperationPattern operationPattern = contentDialogItem.EditedOperationPattern();
                _operationPatterns[_operationPatterns
                    .IndexOf(_operationPatterns
                    .FirstOrDefault(i => i.Id == operationPattern.Id))] = operationPattern;
            };

            await contentDialogItem.ShowAsync();
        }

        public void RemoveOperationPatternFromList(OperationPattern operationPattern) => _operationPatterns.Remove(operationPattern);

        public void AddOperationPatternToList(OperationPattern operationPattern) => _operationPatterns.Insert(0, operationPattern);

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;

            var contentDialogItem = new OperationDetailsContentDialog((OperationPattern)datacontext, "pattern");

            await contentDialogItem.ShowAsync();
        }

        private async void SzablonyListView_ItemClick(object sender, ItemClickEventArgs e) {
            OperationPattern thisPattern = (OperationPattern)e.ClickedItem;

            var contentDialogItem = new OperationDetailsContentDialog(thisPattern, "pattern");

            await contentDialogItem.ShowAsync();
        }
    }
}
