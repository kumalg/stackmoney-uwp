using Finanse.DataAccessLayer;
using Finanse.Elements;
using Finanse.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Dialogs {

    public sealed partial class NewMoneyAccountContentDialog : ContentDialog {

        public NewMoneyAccountContentDialog() {
            this.InitializeComponent();

        }

        private void SetPrimaryButtonEnabled() {
            IsPrimaryButtonEnabled = (NameValue.Text != "");
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {

        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {

        }

        private void CostValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

        }

        private void CostValue_SelectionChanged(object sender, RoutedEventArgs e) {

        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

        }

        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e) {

        }
    }
}
