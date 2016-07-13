﻿using System;
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
                Color = "#FF0b63c7",
                Icon = "\uE806",
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Jedzenie",
                Color = "#FF5bc70b",
                Icon = "",
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Alkohol",
                Color = "#FF138b99",
                Icon = "\uE94C",
            });

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

        public void Category_WydatekTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            var whichColor = OperationCategories.Find(item => item.Name == Category_WydatekTemplate.Text).Color;
            var whichIcon = OperationCategories.Find(item => item.Name == Category_WydatekTemplate.Text).Icon;

            Ellipse_WydatekTemplate.Fill = new SolidColorBrush(GetSolidColorBrush(whichColor).Color);
            Icon_WydatekTemplate.Text = whichIcon;

            Cost_WydatekTemplate.Text = "- " + Wydatek.Cost.ToString("0.00") + " zł";
        }
    }
}
