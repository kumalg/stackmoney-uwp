using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class OperationTemplate : UserControl {

        public decimal Date = 12;

        private Models.OperationPattern Operation {

            get {
                return this.DataContext as Models.OperationPattern;
            }
        }

        public OperationTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();

        }

        private OperationCategory category;

        private OperationCategory Category {
            get {
                if (category == null) {
                    OperationCategory cat = Dal.getOperationCategoryById(Operation.CategoryId);
                    OperationSubCategory subCat = Dal.getOperationSubCategoryById(Operation.SubCategoryId);

                    if (cat != null)
                        category = cat;

                    if (subCat != null)
                        category = subCat;
                }
                return category;
            }
        }


        public void Operation_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            
            /* BO PIERDOLI ŻE NULL WCHODZI */
            if (Operation == null)
                return;

            if (Settings.getAccountEllipseVisibility()) {

                Account moneyAccount = AccountsDal.getAccountById(Operation.MoneyAccountId);

                if (moneyAccount != null)
                    if (!string.IsNullOrEmpty(moneyAccount.ColorKey)) {
                        MoneyAccountEllipse.Visibility = Visibility.Visible;
                        MoneyAccountEllipse.Fill = moneyAccount.SolidColorBrush;
                    }
            }

            OperationCategory cat = Dal.getOperationCategoryById(Operation.CategoryId);
            OperationSubCategory subCat = Dal.getOperationSubCategoryById(Operation.SubCategoryId);
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



            SubCategory_OperationTemplate.Text = "";
            Category_OperationTemplate.Text = "";

            /* WYGLĄD NAZWY KATEGORII */
            if (Settings.getCategoryNameVisibility()) {
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