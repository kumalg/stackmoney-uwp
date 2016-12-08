using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Models {

    public class OperationCategory {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Name  { get; set; }
        //private string _color;
        public string Color {get;set;}
        public string IconId {
            get;set;
        }
        public string Icon { get; set; } 
        public bool VisibleInIncomes { get; set; } 
        public bool VisibleInExpenses { get; set; }

        public ObservableCollection<OperationSubCategory> subCategories = new ObservableCollection<OperationSubCategory>();
        
        public void addSubCategory(OperationSubCategory subCategory) {
            subCategories.Insert(0, subCategory);
        }

        public ObservableCollection<OperationSubCategory> getSubCategories() {
            return new ObservableCollection<OperationSubCategory>(Dal.GetOperationSubCategoriesByBossId(Id));
        }
        
        public FontIcon getFontIcon() {
            if(IconId != "" || IconId != null)
                return (FontIcon)Application.Current.Resources[IconId];
            else
                return (FontIcon)Application.Current.Resources["DefaultEllipseIcon"];
        }
    }
}
