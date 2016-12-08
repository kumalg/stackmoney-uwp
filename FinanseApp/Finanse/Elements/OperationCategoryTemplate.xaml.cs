using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Finanse.Elements {

    public sealed partial class OperationCategoryTemplate : UserControl {
      
        private Models.OperationCategory OperationCategory {

            get {
                return this.DataContext as Models.OperationCategory;
            }
        }

        public OperationCategoryTemplate() {

            this.InitializeComponent();

            this.DataContextChanged += (s, e) => Bindings.Update();            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {

        }

        private void ExpandPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void AddSubCat_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteCat_Click(object sender, RoutedEventArgs e) {

        }

        private void EditCat_Click(object sender, RoutedEventArgs e) {

        }
    }
}
