using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Finanse.Elements {

    public sealed partial class CategoryTemplate : UserControl {
      
        private Models.Category Category {
            get {
                return this.DataContext as Models.Category;
            }
        }

        public CategoryTemplate() {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();            
        }
    }
}
