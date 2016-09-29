using System;
using System.Collections.Generic;
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
using Finanse.Pages;
using System.Reflection;
using System.Globalization;
using Finanse.DataAccessLayer;
using Finanse.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Finanse.Elements {

    public sealed partial class OperationTemplate : UserControl {

        private Models.Operation Operation {

            get {
                return this.DataContext as Models.Operation;
            }
        }

        public OperationTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        private SolidColorBrush GetSolidColorBrush(string hex) {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }

        public void Operation_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {

            /* WYGLĄD KOŁA Z KATEGORIĄ */
            string whichColor = ((SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"]).Color.ToString();
            string whichIcon = ((TextBlock)Application.Current.Resources["DefaultEllipseIcon"]).Text;
            Icon_OperationTemplate.Opacity = 0.2;

            /* BO PIERDOLI ŻE NULL WCHODZI */
            if (Operation == null)
                return;

            MoneyAccount moneyAccount = Dal.GetAllMoneyAccounts().SingleOrDefault(i => i.Id == Operation.MoneyAccountId);
            string moneyAccountColor = string.Empty;

            if (moneyAccount != null)
                moneyAccountColor = moneyAccount.Color;

            if (!string.IsNullOrEmpty(moneyAccountColor))
                MoneyAccountEllipse.Fill = GetSolidColorBrush(moneyAccountColor);
            else
                MoneyAccountEllipse.Visibility = Visibility.Collapsed;

            OperationCategory cat = Dal.GetOperationCategoryById(Operation.CategoryId);
            OperationSubCategory subCat = Dal.GetOperationSubCategoryById(Operation.SubCategoryId);

            /* WCHODZI IKONKA KATEGORII */
            if (cat != null) {
                Icon_OperationTemplate.Opacity = 1;
                whichColor = cat.Color;
                whichIcon = cat.Icon;
            }

            /* PRÓBUJE WEJŚC IKONKA SUBKATEGORII */
            if (subCat != null) {
                Icon_OperationTemplate.Opacity = 1;
                whichColor = subCat.Color;
                whichIcon = subCat.Icon;
            }

            /* GOTOWA IKONKA DO ZAPISANIA */
            Ellipse_OperationTemplate.Fill = new SolidColorBrush(GetSolidColorBrush(whichColor).Color);
            Icon_OperationTemplate.Text = whichIcon;
            Icon_OperationTemplate.FontFamily = new FontFamily(Settings.GetActualIconStyle());

            /* WYGLĄD KOSZTU (CZERWONY Z MINUSEM CZY ZIELONY Z PLUSEM) */
            if (Operation.isExpense) {

                Cost_OperationTemplate.Text = (-Operation.Cost).ToString("C", Settings.GetActualCurrency());
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
            }

            else {

                Cost_OperationTemplate.Text = Operation.Cost.ToString("C", Settings.GetActualCurrency());
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["GreenColorStyle"];
            }

            /* WYGLĄD NAZWY KATEGORII */
            Category_OperationTemplate.Text = "Nie odnaleziono wskazanej kategorii";
            if(cat != null)
                Category_OperationTemplate.Text = cat.Name;
            if (subCat != null)
                SubCategory_OperationTemplate.Text = "  /  " + subCat.Name;
        }
    }
}
