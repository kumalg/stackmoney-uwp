using Finanse.DataAccessLayer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.Models.Categories;

namespace Finanse.Dialogs {

    public sealed partial class NewCategoryContentDialog : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<KeyValuePair<object, object>> _colorBase;
        private List<KeyValuePair<object, object>> ColorBase => _colorBase ??
                                                                (_colorBase = ((ResourceDictionary) Application.Current.Resources["ColorBase"]).ToList());

        private SolidColorBrush Color => (SolidColorBrush)SelectedColor.Value;

        private string ColorKey => SelectedColor.Key.ToString();

        private KeyValuePair<object, object> _selectedColor;

        public KeyValuePair<object, object> SelectedColor {
            get {
                if (_selectedColor.Key == null || _selectedColor.Value == null)
                    _selectedColor = ColorBase.ElementAt(3);

                return _selectedColor;
            }
            set {
                _selectedColor = value;
                RaisePropertyChanged("Color");
            }
        }


        private List<KeyValuePair<object, object>> _iconBase;
        private List<KeyValuePair<object, object>> IconBase => _iconBase ??
                                                               (_iconBase = ((ResourceDictionary) Application.Current.Resources["IconBase"]).ToList());

        private FontIcon Icon => (FontIcon)SelectedIcon.Value;

        private string IconKey => SelectedIcon.Key.ToString();

        private KeyValuePair<object, object> _selectedIcon;

        public KeyValuePair<object, object> SelectedIcon {
            get {
                if (_selectedIcon.Key == null || _selectedIcon.Value == null)
                    _selectedIcon = IconBase.ElementAt(3);

                return _selectedIcon;
            }
            set {
                _selectedIcon = value;
                RaisePropertyChanged("Icon");
            }
        }

        private int _bossCategoryId = -1;
        private int BossCategoryId {
            get {
                return _bossCategoryId;
            }
            set {
                _bossCategoryId = value;
                RaisePropertyChanged("BossCategoryId");
            }
        }

        private readonly List<Category> Categories = Dal.GetAllCategories();

        public Category NewCategoryItem { get; private set; } = new Category();

        private readonly Category _editedCategoryItem;
        private readonly SubCategory _editedCategoryAlwaysAsSubCategory;

        private List<ComboBoxItem> _categoriesInComboBox;
        private List<ComboBoxItem> CategoriesInComboBox {
            get {
                if (_categoriesInComboBox != null)
                    return _categoriesInComboBox;

                _categoriesInComboBox = new List<ComboBoxItem>();
                _categoriesInComboBox.Add(new ComboBoxItem {
                    Content = "Brak",
                    Tag = -1,
                });

                foreach (var categories_ComboBox in Categories) {
                    _categoriesInComboBox.Add(new ComboBoxItem {
                        Content = categories_ComboBox.Name,
                        Tag = categories_ComboBox.Id
                    });
                }

                return _categoriesInComboBox;
            }
        }



        public NewCategoryContentDialog(Category editedCategoryItem) {
            InitializeComponent();
            _editedCategoryItem = new Category(editedCategoryItem);
            _editedCategoryAlwaysAsSubCategory = new SubCategory(editedCategoryItem);
            Title = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("editCategoryString");
            PrimaryButtonText = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("save");

            SelectedColor = ColorBase.FirstOrDefault(i => i.Key.Equals(editedCategoryItem.ColorKey));
            SelectedIcon = IconBase.FirstOrDefault(i => i.Key.Equals(editedCategoryItem.IconKey));

            BossCategoryId = _editedCategoryAlwaysAsSubCategory.BossCategoryId;
            
            NewCategoryItem = new Category(editedCategoryItem);

            /// PÓKI CO NIE DZIAŁA
            /*
            if (editedCategoryItem.CantDelete) {
                Debug.WriteLine("NO NIE MOŻNA");
                CategoryValue.IsEnabled = false;
                VisibleInExpensesToggleButton.IsEnabled = false;
                VisibleInIncomesToggleButton.IsEnabled = false;
            }*/
        }

        public NewCategoryContentDialog(int bossCategoryId) {
            InitializeComponent();
            Title = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("newCategoryString");
            PrimaryButtonText = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("add");

            Category bossCategory = Dal.GetCategoryById(bossCategoryId);
            SelectedColor = ColorBase.FirstOrDefault(i => i.Key.Equals(bossCategory.ColorKey));
            SelectedIcon = IconBase.FirstOrDefault(i => i.Key.Equals(bossCategory.IconKey));

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
            BossCategoryId = bossCategoryId;
        }

        public NewCategoryContentDialog() {
            InitializeComponent();
            Title = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("newCategoryString");
            PrimaryButtonText = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("add");

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
        }



        private object SelectedCategory => CategoriesInComboBox.FirstOrDefault(i => ((int)i.Tag).Equals(BossCategoryId));

        private void SetPrimaryButtonEnabled() {
            IsPrimaryButtonEnabled = _editedCategoryAlwaysAsSubCategory == null
                ? NameNotNullAndCategoryNotInBase
                : OperationNotTheSameAndCategoryNotInBase;
        }

        private bool NameNotNullAndCategoryNotInBase => !( IsThisCategoryInBase() || string.IsNullOrEmpty(NameValue.Text) );

        private bool OperationNotTheSameAndCategoryNotInBase => !( IsThisCategoryInBase() || IsNewOperationTheSame());

        private bool IsThisCategoryInBase() {
            return CategoryValue.SelectedIndex == -1 
                ? Dal.CategoryExistByName(NameValue.Text) 
                : Dal.SubCategoryExistInBaseByName(NameValue.Text, BossCategoryId);
        }

        private bool IsNewOperationTheSame() {
           
            return
                _editedCategoryAlwaysAsSubCategory.BossCategoryId == BossCategoryId &&
                _editedCategoryAlwaysAsSubCategory.ColorKey == SelectedColor.Key.ToString() &&
                _editedCategoryAlwaysAsSubCategory.IconKey == SelectedIcon.Key.ToString() &&
                _editedCategoryAlwaysAsSubCategory.Name == NameValue.Text &&
                !string.IsNullOrEmpty(NameValue.Text) &&
                _editedCategoryAlwaysAsSubCategory.VisibleInExpenses == VisibleInExpensesToggleButton.IsOn &&
                _editedCategoryAlwaysAsSubCategory.VisibleInIncomes == VisibleInIncomesToggleButton.IsOn;
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            NewCategoryItem.Name = NameValue.Text;
            NewCategoryItem.ColorKey = SelectedColor.Key.ToString();
            NewCategoryItem.IconKey = SelectedIcon.Key.ToString();
            NewCategoryItem.VisibleInExpenses = VisibleInExpensesToggleButton.IsOn;
            NewCategoryItem.VisibleInIncomes = VisibleInIncomesToggleButton.IsOn;

            if (CategoryValue.SelectedIndex == -1)
                return;

            SubCategory op = new SubCategory(NewCategoryItem) {
                BossCategoryId = BossCategoryId
            };

            NewCategoryItem = op;
        }

        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetPrimaryButtonEnabled();
        }

        private void SetVisibleInExpensesAndIncomesToNewCategory() {
            NewCategoryItem.VisibleInExpenses = VisibleInExpensesToggleButton.IsOn;
            NewCategoryItem.VisibleInIncomes = VisibleInIncomesToggleButton.IsOn;
        }

        private void VisibleInExpensesToggleButton_Toggled(object sender, RoutedEventArgs e) {
            if (VisibleInIncomesToggleButton != null)
                if (!VisibleInIncomesToggleButton.IsEnabled)
                    VisibleInExpensesToggleButton.IsOn = true;

            if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInIncomesToggleButton.IsOn = true;

            SetVisibleInExpensesAndIncomesToNewCategory();
            SetPrimaryButtonEnabled();
        }

        private void VisibleInIncomesToggleButton_Toggled(object sender, RoutedEventArgs e) {
            if (!VisibleInExpensesToggleButton.IsEnabled)
                VisibleInIncomesToggleButton.IsOn = true;

            else if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInExpensesToggleButton.IsOn = true;

            SetVisibleInExpensesAndIncomesToNewCategory();
            SetPrimaryButtonEnabled();
        }

        private void CategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (((ComboBox)sender).SelectedIndex == 0)
                CategoryValue.SelectedIndex = -1;

            BossCategoryId = (CategoryValue.SelectedIndex == -1) ? -1 : (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;
            SetExpenseAndIncomeToggleButtonsEnabling();

            SetPrimaryButtonEnabled();
            NameValue.Foreground = NameValueForeground;
        }

        private void SetExpenseAndIncomeToggleButtonsEnabling() {
            Category bossCategory = Dal.GetCategoryById(BossCategoryId);

            if (bossCategory != null && (!bossCategory.VisibleInIncomes || !bossCategory.VisibleInExpenses)) {
                VisibleInExpensesToggleButton.IsEnabled = bossCategory.VisibleInExpenses;
                VisibleInIncomesToggleButton.IsEnabled = bossCategory.VisibleInIncomes;

                if (!VisibleInExpensesToggleButton.IsEnabled) {
                    VisibleInExpensesToggleButton.IsOn = false;
                }

                if (!VisibleInIncomesToggleButton.IsEnabled) {
                    VisibleInIncomesToggleButton.IsOn = false;
                }
            }
            else {
                VisibleInExpensesToggleButton.IsEnabled = true;
                VisibleInIncomesToggleButton.IsEnabled = true;
            }
        }

        private void ColorBaseList_SelectionChanged(object sender, SelectionChangedEventArgs e) => SetPrimaryButtonEnabled();

        private void NameValue_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            NameValue.Foreground = NameValueForeground;
        }

        public Brush NameValueForeground => IsThisCategoryInBase()
            ? (SolidColorBrush)Application.Current.Resources["RedColorStyle"]
            : (SolidColorBrush)Application.Current.Resources["Text"];
    }
}
