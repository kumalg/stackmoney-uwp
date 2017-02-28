using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using Finanse.Models;
using Finanse.Models.Categories;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class OperationTemplate : UserControl {

        public decimal Date = 12;

        private Models.OperationPattern Operation => DataContext as OperationPattern;

        public OperationTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();

        }

        private Category category;

        private Category Category {
            get {
                if (category == null) {
                    Category cat = Dal.GetCategoryById(Operation.CategoryId);
                    SubCategory subCat = Dal.GetSubCategoryById(Operation.SubCategoryId);

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

            if (Settings.AccountEllipseVisibility) {

                Account moneyAccount = AccountsDal.GetAccountById(Operation.MoneyAccountId);

                if (!string.IsNullOrEmpty(moneyAccount?.ColorKey)) {
                    MoneyAccountEllipse.Visibility = Visibility.Visible;
                    MoneyAccountEllipse.Fill = moneyAccount.SolidColorBrush;
                }
            }

            Category cat = Dal.GetCategoryById(Operation.CategoryId);
            SubCategory subCat = Dal.GetSubCategoryById(Operation.SubCategoryId);
            SubCategoryNameStackPanel.Visibility = Visibility.Collapsed;

            /* WCHODZI IKONKA KATEGORII */
            if (cat != null && subCat == null) {
                CategoryIcon.Color = cat.Brush;
                CategoryIcon.Glyph = cat.Icon.Glyph;

                if (string.IsNullOrEmpty(Operation.Title)) {
                    Title_OperationTemplate.Text = cat.Name;
                }
            }

            else if (cat != null && subCat != null) {
                CategoryIcon.Color = subCat.Brush;
                CategoryIcon.Glyph = subCat.Icon.Glyph;

                if (string.IsNullOrEmpty(Operation.Title)) {
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
            if (!Settings.CategoryNameVisibility)
                return;

            CategoryNameStackPanel.Visibility = Visibility.Visible;
            Category_OperationTemplate.Text = "Nie znaleziono kategorii";

            if (cat != null)
                Category_OperationTemplate.Text = cat.Name;

            if (subCat == null)
                return;

            SubCategoryNameStackPanel.Visibility = Visibility.Visible;
            SubCategory_OperationTemplate.Text = subCat.Name;
        }
    }
}