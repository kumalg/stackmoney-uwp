using System;
using Finanse.DataAccessLayer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.Models;
using Finanse.Models.Categories;

namespace Finanse.Dialogs {

    public sealed partial class NewCategoryContentDialog : INotifyPropertyChanged {

        private TextBoxEvents _textBoxEvents = new TextBoxEvents();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<KeyValuePair<object, object>> _colorBase;
        private List<KeyValuePair<object, object>> ColorBase => _colorBase ??
                                                                (_colorBase = ((ResourceDictionary)Application.Current.Resources["ColorBase"]).ToList());

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
                RaisePropertyChanged(nameof(Color));
            }
        }


        private List<KeyValuePair<object, object>> _iconBase;
        private List<KeyValuePair<object, object>> IconBase => _iconBase ??
                                                               (_iconBase = ((ResourceDictionary)Application.Current.Resources["IconBase"]).ToList());

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
                RaisePropertyChanged(nameof(Icon));
            }
        }

        private string _bossCategoryGlobalId = string.Empty;
        private string BossCategoryGlobalId {
            get => _bossCategoryGlobalId;
            set {
                _bossCategoryGlobalId = value;
                RaisePropertyChanged(nameof(BossCategoryGlobalId));
            }
        }

        private IEnumerable<Category> _categories = new Category[0];
        private IEnumerable<Category> Categories {
            get => _categories;
            set {
                _categories = value;
                RaisePropertyChanged(nameof(Categories));
                RaisePropertyChanged(nameof(CategoriesInComboBox));
            }
        }// = CategoriesDal.GetAllCategories();//.Where(i => i.GlobalId != _bossCategoryGlobalId && i.GlobalId != _editedCategoryItem.GlobalId);

        public Category NewCategoryItem { get; private set; } = new Category();

        private readonly Category _editedCategoryItem;
        private readonly SubCategory _editedCategoryAlwaysAsSubCategory;

        private List<ComboBoxItem> _categoriesInComboBox;
        private List<ComboBoxItem> CategoriesInComboBox {
            get {
                if (_categoriesInComboBox?.Count() > 1)
                    return _categoriesInComboBox;

                var brak = new ComboBoxItem {
                    Content = "Brak",
                    Tag = -1,
                };
                _categoriesInComboBox = Categories.Select(c => new ComboBoxItem { Content = c.Name, Tag = c.GlobalId }).ToList();
                _categoriesInComboBox.Insert(0, brak);

                return _categoriesInComboBox;
            }
        }



        public NewCategoryContentDialog(Category editedCategoryItem) {
            InitializeComponent();
            Loaded += OnLoadedEditCat;

            _editedCategoryItem = editedCategoryItem.Clone();
            _editedCategoryAlwaysAsSubCategory = new SubCategory(editedCategoryItem);
            Title = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("editCategoryString");
            PrimaryButtonText = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("save");

            SelectedColor = ColorBase.FirstOrDefault(i => i.Key.Equals(editedCategoryItem.ColorKey));
            SelectedIcon = IconBase.FirstOrDefault(i => i.Key.Equals(editedCategoryItem.IconKey));

            BossCategoryGlobalId = _editedCategoryAlwaysAsSubCategory.BossCategoryId;

            NewCategoryItem = editedCategoryItem.Clone();

            /// PÓKI CO NIE DZIAŁA
            /*
            if (editedCategoryItem.CantDelete) {
                Debug.WriteLine("NO NIE MOŻNA");
                CategoryValue.IsEnabled = false;
                VisibleInExpensesToggleButton.IsEnabled = false;
                VisibleInIncomesToggleButton.IsEnabled = false;
            }*/
            
        }

        private async void OnLoadedAddSubCat(object sender, RoutedEventArgs routedEventArgs) {
            Categories = await CategoriesDal.GetAllCategoriesAsync();
            SelectedCategory = CategoriesInComboBox.FirstOrDefault(i => i.Tag.ToString() == BossCategoryGlobalId);
        }

        private async void OnLoadedEditCat(object sender, RoutedEventArgs routedEventArgs) {
            Categories = (await CategoriesDal.GetAllCategoriesAsync()).Where(i => i.GlobalId != _editedCategoryItem.GlobalId);
            SelectedCategory = CategoriesInComboBox.FirstOrDefault(i => i.Tag.ToString() == BossCategoryGlobalId);
        }
        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs) {
            Categories = await CategoriesDal.GetAllCategoriesAsync();
        }

        public NewCategoryContentDialog(string bossCategoryId) {
            InitializeComponent();
            Loaded += OnLoadedAddSubCat;

            Title = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("newCategoryString");
            PrimaryButtonText = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("add");

            Category bossCategory = CategoriesDal.GetCategoryByGlobalId(bossCategoryId);
            SelectedColor = ColorBase.FirstOrDefault(i => i.Key.Equals(bossCategory.ColorKey));
            SelectedIcon = IconBase.FirstOrDefault(i => i.Key.Equals(bossCategory.IconKey));

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
            BossCategoryGlobalId = bossCategory.GlobalId;
        }

        public NewCategoryContentDialog() {
            InitializeComponent();
            Loaded+= OnLoaded;

            Title = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("newCategoryString");
            PrimaryButtonText = new Windows.ApplicationModel.Resources.ResourceLoader().GetString("add");

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
        }


        private ComboBoxItem _selectedCategory;

        private ComboBoxItem SelectedCategory {
            get => _selectedCategory;
            set {
                _selectedCategory = value?.Tag.ToString() == "-1" 
                    ? null 
                    : value;

                RaisePropertyChanged(nameof(SelectedCategory));

                BossCategoryGlobalId = SelectedCategory == null ? string.Empty : SelectedCategory.Tag.ToString();
                SetExpenseAndIncomeToggleButtonsEnabling();

                SetPrimaryButtonEnabled();
                NameValue.Foreground = NameValueForeground;
            }
        } // CategoriesInComboBox.FirstOrDefault(i => i.Tag.ToString() == BossCategoryGlobalId);

        private void SetPrimaryButtonEnabled() {
            IsPrimaryButtonEnabled = _editedCategoryAlwaysAsSubCategory == null
                ? NameNotNullAndCategoryNotInBase
                : OperationNotTheSameAndCategoryNotInBase;
        }

        private bool NameNotNullAndCategoryNotInBase => !(IsThisCategoryInBase() || string.IsNullOrEmpty(NameValue.Text));

        private bool OperationNotTheSameAndCategoryNotInBase => !(IsThisCategoryInBase() || IsNewOperationTheSame());

        private bool IsThisCategoryInBase() {
            if (_editedCategoryItem != null && _editedCategoryItem.Name.Trim() == NameValue.Text.Trim())
                return false;
            /*
            return CategoryValue.SelectedIndex == -1 
                ? CategoriesDal.CategoryExistInBaseByName(NameValue.Text) 
                : CategoriesDal.SubCategoryExistInBaseByName(NameValue.Text, BossCategoryGlobalId);*/
            return CategoriesDal.CategoryExistInBaseByName(NameValue.Text);
        }

        private bool IsNewOperationTheSame() {

            return
                _editedCategoryAlwaysAsSubCategory.BossCategoryId == BossCategoryGlobalId &&
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

            if (SelectedCategory == null) {
                if (NewCategoryItem is SubCategory) {
                    var newSub = NewCategoryItem as SubCategory;
                    NewCategoryItem = newSub.ToCategory();
                }
                return;
            }

            SubCategory op = new SubCategory(NewCategoryItem) {
                BossCategoryId = BossCategoryGlobalId
            };

            NewCategoryItem = op;
        }

        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetPrimaryButtonEnabled();
            NameValue.Foreground = NameValueForeground;
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
            //if (SelectedCategory.Tag.ToString() == "-1")
            //    SelectedCategory = null;
            //   // CategoryValue.SelectedIndex = -1;

            //BossCategoryGlobalId = (SelectedCategory == null) ? string.Empty : ((ComboBoxItem)SelectedCategory).Tag.ToString();
            //SetExpenseAndIncomeToggleButtonsEnabling();

            //SetPrimaryButtonEnabled();
            //NameValue.Foreground = NameValueForeground;
        }

        private void SetExpenseAndIncomeToggleButtonsEnabling() {
            Category bossCategory = CategoriesDal.GetCategoryByGlobalId(BossCategoryGlobalId);

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

        public Brush NameValueForeground => IsThisCategoryInBase()
            ? (SolidColorBrush)Application.Current.Resources["RedColorStyle"]
            : (SolidColorBrush)Application.Current.Resources["Text"];
    }
}