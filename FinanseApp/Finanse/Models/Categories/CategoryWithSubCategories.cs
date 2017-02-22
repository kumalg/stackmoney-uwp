using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.Categories {
    public class CategoryWithSubCategories : INotifyPropertyChanged {
        private Category category;
        public Category Category {
            get {
                return category;
            }
            set {
                category = value;
                OnPropertyChanged("Category");
            }
        }

        public ObservableCollection<SubCategory> SubCategories = new ObservableCollection<SubCategory>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public override int GetHashCode() {
            return this.Category.GetHashCode() * this.Category.Id;
        }
        public override bool Equals(object o) {
            if (o == null || !(o is CategoryWithSubCategories))
                return false;

            return 
                this.Category
                .Equals((o as CategoryWithSubCategories).Category);
        }
    }
}
