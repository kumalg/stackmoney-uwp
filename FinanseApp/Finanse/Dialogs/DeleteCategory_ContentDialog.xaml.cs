using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {
    public sealed partial class DeleteCategory_ContentDialog : ContentDialog {
        ObservableCollection<Category> Categories;
        Category category;
        SubCategory subCategory;
        public DeleteCategory_ContentDialog(ObservableCollection<Category> Categories, Category category, SubCategory subCategory) {
            this.InitializeComponent();
            this.Categories = Categories;
            this.category = category;
            this.subCategory = subCategory;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        }
    }
}
