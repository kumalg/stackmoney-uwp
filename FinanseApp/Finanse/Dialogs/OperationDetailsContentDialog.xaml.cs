using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Finanse.DataAccessLayer;
using Finanse.Models;
using Finanse.Models.Categories;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Dialogs {

    public sealed partial class OperationDetailsContentDialog : ContentDialog {
        

        public OperationDetailsContentDialog(OperationPattern editedOperation, string whichOption) {

            this.InitializeComponent();

            if (whichOption == "pattern")
                Title = "Szczegóły szablonu";

            if (editedOperation.isExpense) {
                CostValue.Text = (-editedOperation.Cost).ToString("C", Settings.GetActualCultureInfo());
                CostValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
            }
            else {
                CostValue.Text = editedOperation.Cost.ToString("C", Settings.GetActualCultureInfo());
                CostValue.Foreground = (SolidColorBrush)Application.Current.Resources["GreenColorStyle"];
            }

            NameValue.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(editedOperation.Title)) {
                NameValue.Text = editedOperation.Title;
                NameValue.Visibility = Visibility.Visible;
            }

            DateValuePanel.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty((editedOperation as Operation)?.Date)) {
                DateValue.Text = string.Format("{0:dddd, dd MMMM yyyy}", Convert.ToDateTime(((Operation)editedOperation).Date));
                DateValuePanel.Visibility = Visibility.Visible;
            }

            Category cat = Dal.GetCategoryById(editedOperation.CategoryId);
            SubCategory subCat = Dal.GetSubCategoryById(editedOperation.SubCategoryId);
            Account account = AccountsDal.GetAccountById(editedOperation.MoneyAccountId);

            /* KATEGORIA */
            CategoryValuePanel.Visibility = Visibility.Collapsed;
            if (cat != null) {
                CategoryValue.Text = cat.Name;
                CategoryValuePanel.Visibility = Visibility.Visible;
            }

            /* PODKATEGORIA */
            SubCategoryValuePanel.Visibility = Visibility.Collapsed;
            if (subCat != null) {
                SubCategoryValue.Text = subCat.Name;
                SubCategoryValuePanel.Visibility = Visibility.Visible;
            }

            /* FORMA PŁATNOŚCI */
            PayFormPanel.Visibility = Visibility.Collapsed;
            if (account != null) {
                PayForm.Text = account.Name;
                PayFormPanel.Visibility = Visibility.Visible;

                if (account is CashAccount)
                    PayFormIcon.Glyph = "";
                else if (account is BankAccount)
                    PayFormIcon.Glyph = "";
                else
                    PayFormIcon.Glyph = "";
            }

            /* WIĘCEJ INFORMACJI */
            if (!string.IsNullOrEmpty(editedOperation.MoreInfo)) {
                MoreInfo.Text = editedOperation.MoreInfo;
            }
            else
                MoreInfoPanel.Visibility = Visibility.Collapsed;
        }
        
        private void Exit_Click(object sender, RoutedEventArgs e) {
            Hide();
        }
    }
}
