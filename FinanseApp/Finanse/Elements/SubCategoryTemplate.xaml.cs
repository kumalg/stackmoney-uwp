using Windows.UI.Xaml.Controls;
using Finanse.Models.Categories;

namespace Finanse.Elements {

    public sealed partial class SubCategoryTemplate : UserControl {
      
        private SubCategory SubCategory => DataContext as SubCategory;

        public SubCategoryTemplate() {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();       
        }
    }
}
