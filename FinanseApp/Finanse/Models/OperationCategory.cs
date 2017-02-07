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
        public string IconKey { get; set; }
        public bool VisibleInIncomes { get; set; } 
        public bool VisibleInExpenses { get; set; }


        public SolidColorBrush Color {
            get {
                return string.IsNullOrEmpty(ColorKey) ?
                    (SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"] :
                    (SolidColorBrush)(((ResourceDictionary)Application.Current.Resources["ColorBase"]).FirstOrDefault(i => i.Key.Equals(ColorKey)).Value);
            }
        }
        public FontIcon Icon {
            get {
                return string.IsNullOrEmpty(IconKey) ?
                    (FontIcon)Application.Current.Resources["DefaultEllipseIcon"] :
                    (FontIcon)(((ResourceDictionary)Application.Current.Resources["IconBase"]).FirstOrDefault(i => i.Key.Equals(IconKey)).Value);
            }
        }
        public OperationCategory() {
        }

        public OperationCategory(OperationCategory previousCategory) {
            Id = previousCategory.Id;
            IconKey = previousCategory.IconKey;
            ColorKey = previousCategory.ColorKey;
            Name = previousCategory.Name;
            VisibleInExpenses = previousCategory.VisibleInExpenses;
            VisibleInIncomes = previousCategory.VisibleInIncomes;
        }

        public override int GetHashCode() {
            return Name.GetHashCode() * Id;
        }
        public override bool Equals(object o) {
            if (o == null || !(o is OperationCategory))
                return false;

            OperationCategory secondCategory = o as OperationCategory;

            return
                ColorKey == secondCategory.ColorKey &&
                IconKey == secondCategory.IconKey &&
                Name == secondCategory.Name &&
                VisibleInExpenses == secondCategory.VisibleInExpenses &&
                VisibleInIncomes == secondCategory.VisibleInIncomes;
        }
    }
}
