using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Finanse.Models.Categories;

namespace Finanse.Elements {

    public sealed partial class CategoryTemplate : UserControl {
      
        private Category Category => DataContext as Category;

        public CategoryTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();            
        }
    }
}
