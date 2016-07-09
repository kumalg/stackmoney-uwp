using System;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse
{
    public sealed partial class Wplyw : UserControl
    {
        /*
       public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(Wplyw), null);
       public string Glyph
       {
           get { return GetValue(GlyphProperty) as string; }
           set { SetValue(GlyphProperty, value); }
       }

       public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Wplyw), null);
       public string Title
       {
           get { return GetValue(TitleProperty) as string; }
           set { SetValue(TitleProperty, value); }
       }

       public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(string), typeof(Wplyw), null);
       public string Type
       {
           get { return GetValue(TypeProperty) as string; }
           set { SetValue(TypeProperty, value); }
       }

       public static readonly DependencyProperty CostProperty = DependencyProperty.Register("Cost", typeof(string), typeof(Wplyw), null);
       public string Cost
       {
           get { return GetValue(CostProperty) as string; }
           set { SetValue(CostProperty, value); }
       }*/

        public Wplyw()
        {
            this.InitializeComponent();
            //this.DataContext = this;
        }
    }
}
