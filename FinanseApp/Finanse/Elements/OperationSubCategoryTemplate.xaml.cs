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

    public sealed partial class OperationSubCategoryTemplate : UserControl {
      
        private Elements.OperationSubCategory OperationSubCategory {

            get {
                return this.DataContext as Elements.OperationSubCategory;
            }
        }

        public OperationSubCategoryTemplate() {

            this.InitializeComponent();

            this.DataContextChanged += (s, e) => Bindings.Update();            
        }
    }
}
