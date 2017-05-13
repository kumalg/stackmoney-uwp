using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Finanse.Models.Categories {
    public class CategoryWithSubCategories : INotifyPropertyChanged {
        private Category _category;
        public Category Category {
            get => _category;
            set {
                _category = value;
                OnPropertyChanged("Category");
            }
        }
        /*
        private ObservableCollection<SubCategory> _subCategories; //TODO wypierdalal nullem
        public ObservableCollection<SubCategory> SubCategories {
            get {
                return _subCategories ?? ( _subCategories = new ObservableCollection<SubCategory>() );
            }
            set {
                _subCategories = value;
                OnPropertyChanged("SubCategories");
            }
        }
        */

        public ObservableCollection<SubCategory> SubCategories { get; set; } = new ObservableCollection<SubCategory>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
