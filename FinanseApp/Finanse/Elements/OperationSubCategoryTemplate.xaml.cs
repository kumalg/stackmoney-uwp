using Windows.UI.Xaml.Controls;

namespace Finanse.Elements {

    public sealed partial class OperationSubCategoryTemplate : UserControl {
      
        private Models.OperationSubCategory OperationSubCategory {

            get {
                return this.DataContext as Models.OperationSubCategory;
            }
        }

        public OperationSubCategoryTemplate() {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();       
        }
    }
}
