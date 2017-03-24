using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.Categories {
    public class CategoryWithSubCategories : INotifyPropertyChanged {
        private Category _category;
        public Category Category {
            get {
                return _category;
            }
            set {
                _category = value;
                OnPropertyChanged("Category");
            }
        }

        private ObservableCollection<SubCategory> _subCategories;
        public ObservableCollection<SubCategory> SubCategories {
            get {
                return _subCategories ?? ( _subCategories = new ObservableCollection<SubCategory>() );
            }
            set {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public override int GetHashCode() {
            return Category.GetHashCode() * Category.Id;
        }
        public override bool Equals(object o) {
            if (!(o is CategoryWithSubCategories))
                return false;

            return
                Category
                .Equals(((CategoryWithSubCategories)o).Category);
        }
    }
}
