using Windows.UI.Xaml;
using Finanse.Models;
using Finanse.Models.Categories;
using Finanse.Models.Operations;

namespace Finanse.Elements {

    public sealed partial class OperationTemplate {

        //public decimal Date = 12;

        private OperationPattern Operation => DataContext as OperationPattern;

        private readonly Visibility _accountEllipseVisibile = Settings.AccountEllipseVisibility 
            ? Visibility.Visible 
            : Visibility.Collapsed;

        private readonly Visibility categoryNameVisibility = Settings.CategoryNameVisibility
            ? Visibility.Visible
            : Visibility.Collapsed;

        private bool HaveSubCategory => Operation?.GeneralCategory is SubCategory;

        public OperationTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}