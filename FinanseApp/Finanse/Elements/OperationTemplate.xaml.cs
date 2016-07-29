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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Finanse.Elements {

    public sealed partial class OperationTemplate : UserControl {

        private List<OperationCategory> OperationCategories = new List<OperationCategory>();

        string path;
        SQLite.Net.SQLiteConnection conn;

        private Elements.Operation Operation {

            get {
                return this.DataContext as Elements.Operation;
            }
        }

        public OperationTemplate() {

            this.InitializeComponent();

            OperationCategory operationCategoryItem;

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            foreach (var message in conn.Table<OperationCategory>()) {
                operationCategoryItem = message;

                foreach (var submessage in conn.Table<OperationSubCategory>()) {
                    if (submessage.BossCategory == message.Name) {
                        operationCategoryItem.addSubCategory(submessage);
                    }
                }
                OperationCategories.Add(operationCategoryItem);
            }

            this.DataContextChanged += (s, e) => Bindings.Update();            
        }
        

        public SolidColorBrush GetSolidColorBrush(string hex) {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }

        public void Category_OperationTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {

            /* WYGLĄD KOŁA Z KATEGORIĄ */
            string whichColor = null;
            string whichIcon = null;
            OperationCategory whichCat = null;

            if (Operation.SubCategory == null) {
                whichColor = OperationCategories.Find(item => item.Name == Operation.Category).Color;
                whichIcon = OperationCategories.Find(item => item.Name == Operation.Category).Icon;
            }
            else {
                whichCat = OperationCategories.Find(item => item.Name == Operation.Category);

                whichColor = whichCat.subCategories.Single(item => item.Name == Operation.SubCategory).Color;
                whichIcon = whichCat.subCategories.Single(item => item.Name == Operation.SubCategory).Icon;
            }

            Ellipse_OperationTemplate.Fill = new SolidColorBrush(GetSolidColorBrush(whichColor).Color);
            Icon_OperationTemplate.Text = whichIcon;

            /* WYGLĄD KOSZTU (CZERWONY Z MINUSEM CZY ZIELONY Z PLUSEM) */
            if (Operation.ExpenseOrIncome == "expense") {
                Cost_OperationTemplate.Text = "- " + Operation.Cost.ToString("0.00") + " zł";
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
            }
            else {
                Cost_OperationTemplate.Text = "+ " + Operation.Cost.ToString("0.00") + " zł";
                Cost_OperationTemplate.Foreground = (SolidColorBrush)Application.Current.Resources["GreenColorStyle"];
            }

            /* CZY WYŚWIETLAĆ PODKATEGORIĘ */
            if (Operation.SubCategory != null)
                SubCategory_OperationTemplate.Text = "  /  " + Operation.SubCategory;
        }
    }
}
