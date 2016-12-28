using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using Finanse.Models;

namespace Finanse.Elements {

    public sealed partial class OperationTemplate : UserControl {

        private Models.OperationPattern Operation {

            get {
                return this.DataContext as Models.OperationPattern;
            }
        }

        public OperationTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
           
        }

        public void Operation_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {

            /* BO PIERDOLI ŻE NULL WCHODZI */
            if (Operation == null)
                return;

            if (Settings.GetAccountEllipseVisibility()) {

                MoneyAccount moneyAccount = Dal.GetMoneyAccountById(Operation.MoneyAccountId);

                if (moneyAccount != null)
                    if (!string.IsNullOrEmpty(moneyAccount.Color)) {
                        MoneyAccountEllipse.Visibility = Visibility.Visible;
                        MoneyAccountEllipse.Fill = Functions.GetSolidColorBrush(moneyAccount.Color);
                    }
            }

            OperationCategory cat = Dal.GetOperationCategoryById(Operation.CategoryId);
            OperationSubCategory subCat = Dal.GetOperationSubCategoryById(Operation.SubCategoryId);
            SubCategoryNameStackPanel.Visibility = Visibility.Collapsed;

            /* WCHODZI IKONKA KATEGORII */
            if (cat != null && subCat == null) {
                CategoryIcon.Color = cat.Color;
                CategoryIcon.Glyph = cat.Icon.Glyph;

                if (String.IsNullOrEmpty(Operation.Title)) {
                    Title_OperationTemplate.Text = cat.Name;
                }
            }

            else if (cat != null && subCat != null) {
                CategoryIcon.Color = subCat.Color;
                CategoryIcon.Glyph = subCat.Icon.Glyph;

                if (String.IsNullOrEmpty(Operation.Title)) {
                    Title_OperationTemplate.Text = subCat.Name;
                }
            }
            
            else {
                CategoryIcon.Color = (SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"];
                CategoryIcon.Glyph = ((FontIcon)Application.Current.Resources["DefaultEllipseIcon"]).Glyph;
            }

            /* WYGLĄD KOSZTU (CZERWONY Z MINUSEM CZY ZIELONY Z PLUSEM) */
            if (Operation.isExpense) {

                Cost_OperationTemplate.Text = (-Operation.Cost).ToString("C", Settings.GetActualCurrency());
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
            }

            else {

                Cost_OperationTemplate.Text = Operation.Cost.ToString("C", Settings.GetActualCurrency());
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["GreenColorStyle"];
            }

            SubCategory_OperationTemplate.Text = "";
            Category_OperationTemplate.Text = "";

            /* WYGLĄD NAZWY KATEGORII */
            if (Settings.GetCategoryNameVisibility()) {
                CategoryNameStackPanel.Visibility = Visibility.Visible;
                Category_OperationTemplate.Text = "Nie znaleziono kategorii";
                if (cat != null)
                    Category_OperationTemplate.Text = cat.Name;

                if (subCat != null) {
                    SubCategoryNameStackPanel.Visibility = Visibility.Visible;
                    SubCategory_OperationTemplate.Text = subCat.Name;
                }
            }
        }
    }
}
