using Windows.UI.Xaml.Controls;

namespace Finanse.Elements {

    public sealed partial class SubCategoryTemplate : UserControl {
      
        private Models.SubCategory SubCategory {

            get {
                return this.DataContext as Models.SubCategory;
            }
        }

        public SubCategoryTemplate() {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();       
        }
    }
}
