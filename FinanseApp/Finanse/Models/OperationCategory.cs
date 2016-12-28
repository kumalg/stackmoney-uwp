using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;
using System.Collections.ObjectModel;
using Windows.UI;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;

namespace Finanse.Models {

    public class OperationCategory {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } 
        public string Name  { get; set; }
        public string ColorKey { get; set; }
        public SolidColorBrush Color {
            get {
                return string.IsNullOrEmpty(ColorKey) ?
                    (SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"] :
                    (SolidColorBrush)(((ResourceDictionary)Application.Current.Resources["ColorBase"]).FirstOrDefault(i => i.Key.Equals(ColorKey)).Value);
            }
        }
        public string IconKey { get; set; }
        public FontIcon Icon {
            get {
                return string.IsNullOrEmpty(IconKey) ? 
                    (FontIcon)Application.Current.Resources["DefaultEllipseIcon"] : 
                    (FontIcon)(((ResourceDictionary)Application.Current.Resources["IconBase"]).FirstOrDefault(i => i.Key.Equals(IconKey)).Value);
            }
        } 
        public bool VisibleInIncomes { get; set; } 
        public bool VisibleInExpenses { get; set; }

        public ObservableCollection<OperationSubCategory> subCategories = new ObservableCollection<OperationSubCategory>();
        
        public void addSubCategory(OperationSubCategory subCategory) {
            subCategories.Insert(0, subCategory);
        }

        public ObservableCollection<OperationSubCategory> getSubCategories() {
            return new ObservableCollection<OperationSubCategory>(Dal.GetOperationSubCategoriesByBossId(Id));
        }
    }
}
