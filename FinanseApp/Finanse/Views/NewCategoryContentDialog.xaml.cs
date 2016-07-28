using Finanse.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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

        SQLite.Net.SQLiteConnection conn;

        public OperationSubCategory newOperationSubCategoryItem;

        public NewCategoryContentDialog(ObservableCollection<OperationCategory> OperationCategories, ObservableCollection<OperationCategory> OperationSubCategories, SQLite.Net.SQLiteConnection conn) {

            this.InitializeComponent();

            this.OperationCategories = OperationCategories;
            this.OperationSubCategories = OperationSubCategories;
            this.conn = conn;

            /* DODAWANIE KATEGORII DO COMBOBOX'A */
            foreach (OperationCategory OperationCategories_ComboBox in OperationCategories) {

                CategoryValue.Items.Add(new ComboBoxItem {
                    Content = OperationCategories_ComboBox.Name
                });
            }
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            if (CategoryValue.SelectedIndex != -1) {
                newOperationSubCategoryItem = new OperationSubCategory {
                    Name = NameValue.Text,
                    Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                    Icon = CategoryIcon.Glyph,
                    BossCategory = ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString(),
                    VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                    VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                };
                foreach (var message in conn.Table<OperationCategory>()) {
                    if (message.Name == newOperationSubCategoryItem.BossCategory) {
                        conn.Insert(newOperationSubCategoryItem);
                    }
                }
                OperationCategories.Single(x => x.Name == ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString()).addSubCategory(newOperationSubCategoryItem);
            }

            else {
                OperationCategories.Add(new OperationCategory() {
                    Name = NameValue.Text,
                    Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                    Icon = CategoryIcon.Glyph,
                    VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                    VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                });
                conn.Insert(new OperationCategory() {
                    Name = NameValue.Text,
                    Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                    Icon = CategoryIcon.Glyph,
                    VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                    VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
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
    }
}
