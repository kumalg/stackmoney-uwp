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
using Finanse.Views;
using System.Reflection;
using System.Globalization;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Finanse.Elements {

    public sealed partial class OperationTemplate : UserControl {

        private string path;
        private SQLite.Net.SQLiteConnection conn;
        private Settings settings;

        private Elements.Operation Operation {

            get {
                return this.DataContext as Elements.Operation;
            }
        }

        public OperationTemplate() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            settings = conn.Table<Settings>().ElementAt(0);

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

            /* BO PIERDOLI ŻE NULL WCHODZI */
            if (Operation == null)
                return;

            /* WCHODZI IKONKA KATEGORII */
            Icon_OperationTemplate.Opacity = 0.2;
            foreach (OperationCategory item in conn.Table<OperationCategory>()) {
                if (item.Id == Operation.CategoryId) {
                    Icon_OperationTemplate.Opacity = 1;
                    whichColor = item.Color;
                    whichIcon = item.Icon;
                    break;
                }
            }

            /* PRÓBUJE WEJŚC IKONKA SUBKATEGORII */
            foreach (OperationSubCategory item in conn.Table<OperationSubCategory>()) {
                if (item.OperationCategoryId == Operation.SubCategoryId) {
                    Icon_OperationTemplate.Opacity = 1;
                    whichColor = item.Color;
                    whichIcon = item.Icon;
                    break;
                }
            }

            /* GOTOWA IKONKA DO ZAPISANIA */
            Ellipse_OperationTemplate.Fill = new SolidColorBrush(GetSolidColorBrush(whichColor).Color);
            Icon_OperationTemplate.Text = whichIcon;

            /* WYGLĄD KOSZTU (CZERWONY Z MINUSEM CZY ZIELONY Z PLUSEM) */
            if (Operation.isExpense) {

                Cost_OperationTemplate.Text = (-Operation.Cost).ToString("C", new CultureInfo(settings.CultureInfoName));
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
            }

            else {

                Cost_OperationTemplate.Text = Operation.Cost.ToString("C", new CultureInfo(settings.CultureInfoName));
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["GreenColorStyle"];
            }

            /* WYGLĄD KATEGORII */
            Category_OperationTemplate.Text = "Nie odnaleziono wskazanej kategorii";
            foreach (OperationCategory item in conn.Table<OperationCategory>()) {

                if (item.Id == Operation.CategoryId) {

                    Category_OperationTemplate.Text = item.Name;

                    if (Operation.SubCategoryId == -1)
                        break;

                    foreach (OperationSubCategory subItem in conn.Table<OperationSubCategory>()) {
                        if (subItem.OperationCategoryId == Operation.SubCategoryId) {
                            SubCategory_OperationTemplate.Text = "  /  " + subItem.Name;
                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}
