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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class NewCategoryContentDialog : ContentDialog {

        ObservableCollection<OperationCategory> OperationCategories;
        ObservableCollection<OperationCategory> OperationSubCategories;

        public NewCategoryContentDialog(ObservableCollection<OperationCategory> OperationCategories, ObservableCollection<OperationCategory> OperationSubCategories) {

            this.InitializeComponent();

            this.OperationCategories = OperationCategories;
            this.OperationSubCategories = OperationSubCategories;
        }

        private void NewCategory_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            if (CategoryValue.SelectedIndex != -1) {
                OperationSubCategories.Add(new OperationCategory() {

                    Name = "Gowno",
                    Color = "#234567",
                    Icon = "\uE700",
                    BossCategory = "Transport"
                });
            }
            else {
                OperationCategories.Add(new OperationCategory() {

                    Name = "Gowno",
                    Color = "#234567",
                    Icon = "\uE700",

                });
            }
        }

        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e) {
            var button = sender as RadioButton;
            CategoryCircle.Fill = button.Background;
        }

        private void RadioButtonIcon_Checked(object sender, RoutedEventArgs e) {
            var button = sender as RadioButton;
            CategoryIcon.Glyph = button.Content.ToString();
            CategoryIcon.FontFamily = button.FontFamily;
        }

        private void ComboBox_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {

        }
    }
}
