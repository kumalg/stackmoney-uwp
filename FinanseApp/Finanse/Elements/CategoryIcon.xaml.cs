﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Finanse.Elements {
    public sealed partial class CategoryIcon : UserControl {

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(CategoryIcon), null);

        public string Glyph {
            get {
                return GetValue(GlyphProperty) as string;
            }

            set {
                SetValue(GlyphProperty, value);
            }
        }

        public double HalfSize {
            get {
                return Width / 3.0;
            }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(CategoryIcon), null);

        public SolidColorBrush Color {

            get {
                return GetValue(ColorProperty) as SolidColorBrush;
            }

            set {
                SetValue(ColorProperty, value);
            }
        }

        public CategoryIcon() {
            this.InitializeComponent();
            this.DataContext = this;
        }
    }
}
