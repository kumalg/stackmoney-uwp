using Windows.UI.Xaml.Controls;
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
