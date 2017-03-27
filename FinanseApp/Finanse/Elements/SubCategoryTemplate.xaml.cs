using Finanse.Models.Categories;

namespace Finanse.Elements {

    public sealed partial class SubCategoryTemplate {
      
        private SubCategory SubCategory => DataContext as SubCategory;

        public SubCategoryTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();       
        }
    }
}
