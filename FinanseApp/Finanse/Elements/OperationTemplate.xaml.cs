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
            if (conn.Table<OperationCategory>().Any(item => item.Id == Operation.CategoryId)) {

                whichColor = conn.Table<OperationCategory>().Single(item => item.Id == Operation.CategoryId).Color;
                whichIcon = conn.Table<OperationCategory>().Single(item => item.Id == Operation.CategoryId).Icon;
            }

            /* GDY NIE WEJDZIE */
            else
                Icon_OperationTemplate.Opacity = 0.2;

            /* PRÓBUJE WEJŚC IKONKA SUBKATEGORII */
            if (conn.Table<OperationSubCategory>().Any(item => item.OperationCategoryId == Operation.SubCategoryId)) {

                Icon_OperationTemplate.Opacity = 1;
                whichColor = conn.Table<OperationSubCategory>().Single(item => item.OperationCategoryId == Operation.SubCategoryId).Color;
                whichIcon = conn.Table<OperationSubCategory>().Single(item => item.OperationCategoryId == Operation.SubCategoryId).Icon;
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


            /* TYTUŁ KATEGORII */
            if (conn.Table<OperationCategory>().Any(cat => cat.Id == Operation.CategoryId)) {

                Category_OperationTemplate.Text = conn.Table<OperationCategory>().Single(cat => cat.Id == Operation.CategoryId).Name;

                /* CZY WYŚWIETLAĆ PODKATEGORIĘ */
                if (Operation.SubCategoryId != -1 && conn.Table<OperationSubCategory>().Any(subcat => subcat.OperationCategoryId == Operation.SubCategoryId))
                    SubCategory_OperationTemplate.Text = "  /  " + conn.Table<OperationSubCategory>().Single(subcat => subcat.OperationCategoryId == Operation.SubCategoryId).Name;
            }

            else
                Category_OperationTemplate.Text = "Nie odnaleziono wskazanej kategorii";
        }
    }
}
