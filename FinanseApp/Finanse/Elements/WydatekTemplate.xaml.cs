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

    public sealed partial class WydatekTemplate : UserControl {

        public static List<OperationCategory> OperationCategories = new List<OperationCategory>();
        
        public Elements.Wydatek Wydatek {

            get {
                return this.DataContext as Elements.Wydatek;
            }
        }
        
        public WydatekTemplate() {

            this.InitializeComponent();

            OperationCategories.Add(new OperationCategory {
                Name = "Transport",
                Color = "#FF7F00FF",
                Icon = "&#xE700;",
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Jedzenie",
                Color = "#FF7F22FF",
                Icon = "&#xE700;",
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Alkohol",
                Color = "#FF7F2222",
                Icon = "&#xE700;",
            });
            
            this.DataContextChanged += (s, e) => Bindings.Update();
            
            Titletitle.Text = Categoria.Text + " yyy coo";
            /*
            string yolox;
            if (yoloff == null || yoloff == "") {
                yolox = "Red";
            }
            else
                yolox = "Green";

            var whichColor = OperationCategories.Find(item => item.Name == yolox).Color;

            Ellipse.Fill = new SolidColorBrush(GetSolidColorBrush(whichColor).Color);*/
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
    }
}
