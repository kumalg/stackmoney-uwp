using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.Models.Categories;

namespace Finanse.Dialogs {

    public sealed partial class NewCategoryContentDialog : ContentDialog, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<KeyValuePair<object, object>> colorBase;
        private List<KeyValuePair<object, object>> ColorBase => colorBase ??
                                                                (colorBase = ((ResourceDictionary) Application.Current.Resources["ColorBase"]).ToList());

        private SolidColorBrush Color => (SolidColorBrush)SelectedColor.Value;

        private string ColorKey => SelectedColor.Key.ToString();

        private KeyValuePair<object, object> selectedColor;

        public KeyValuePair<object, object> SelectedColor {
            get {
                if (selectedColor.Key == null || selectedColor.Value == null)
                    selectedColor = ColorBase.ElementAt(3);

                return selectedColor;
            }
            set {
                selectedColor = value;
                RaisePropertyChanged("Color");
            }
        }


        private List<KeyValuePair<object, object>> iconBase;
        private List<KeyValuePair<object, object>> IconBase => iconBase ??
                                                               (iconBase = ((ResourceDictionary) Application.Current.Resources["IconBase"]).ToList());

        private FontIcon Icon => (FontIcon)SelectedIcon.Value;

        private string IconKey => SelectedIcon.Key.ToString();

        private KeyValuePair<object, object> selectedIcon;

        public KeyValuePair<object, object> SelectedIcon {
            get {
                if (selectedIcon.Key == null || selectedIcon.Value == null)
                    selectedIcon = IconBase.ElementAt(3);

                return selectedIcon;
            }
            set {
                selectedIcon = value;
                RaisePropertyChanged("Icon");
            }
        }

        private int bossCategoryId = -1;
        private int BossCategoryId {
            get {
                return bossCategoryId;
            }
            set {
                bossCategoryId = value;
                RaisePropertyChanged("BossCategoryId");
            }
        }

        private readonly List<Category> Categories = Dal.GetAllCategories();

        public Category NewCategoryItem { get; private set; } = new Category();

        private readonly Category editedCategoryItem;
        private readonly SubCategory editedCategoryAlwaysAsSubCategory;

        private List<ComboBoxItem> categoriesInComboBox;
        private List<ComboBoxItem> CategoriesInComboBox {
            get {
                if (categoriesInComboBox != null)
                    return categoriesInComboBox;

                categoriesInComboBox = new List<ComboBoxItem>();
                categoriesInComboBox.Add(new ComboBoxItem {
                    Content = "Brak",
                    Tag = -1,
                });

                foreach (var categories_ComboBox in Categories) {
                    categoriesInComboBox.Add(new ComboBoxItem {
                        Content = categories_ComboBox.Name,
                        Tag = categories_ComboBox.Id
                    });
                }

                return categoriesInComboBox;
            }
        }



        public NewCategoryContentDialog(Category editedCategoryItem) {
            this.InitializeComponent();
            this.editedCategoryItem = new Category(editedCategoryItem);
            this.editedCategoryAlwaysAsSubCategory = new SubCategory(editedCategoryItem);
            Title = "Edytowanie kategorii";

            SelectedColor = ColorBase.FirstOrDefault(i => i.Key.Equals(editedCategoryItem.ColorKey));
            SelectedIcon = IconBase.FirstOrDefault(i => i.Key.Equals(editedCategoryItem.IconKey));

            BossCategoryId = editedCategoryAlwaysAsSubCategory.BossCategoryId;
            
            NewCategoryItem = new Category(editedCategoryItem);
        }

        public NewCategoryContentDialog(int BossCategoryId) {
            InitializeComponent();
            Title = "Nowa kategoria";

            Category bossCategory = Dal.GetCategoryById(BossCategoryId);
            SelectedColor = ColorBase.FirstOrDefault(i => i.Key.Equals(bossCategory.ColorKey));
            SelectedIcon = IconBase.FirstOrDefault(i => i.Key.Equals(bossCategory.IconKey));

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
            this.BossCategoryId = BossCategoryId;
        }

        public NewCategoryContentDialog() {
            this.InitializeComponent();
            Title = "Nowa kategoria";

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
        }



        private object SelectedCategory => CategoriesInComboBox.FirstOrDefault(i => ((int)i.Tag).Equals(BossCategoryId));

        private void SetPrimaryButtonEnabled() {
            IsPrimaryButtonEnabled = editedCategoryAlwaysAsSubCategory == null ? !string.IsNullOrEmpty(NameValue.Text) : !IsNewOperationTheSame();
        }

        private bool IsNewOperationTheSame() {
           
            return
                editedCategoryAlwaysAsSubCategory.BossCategoryId == BossCategoryId &&
                editedCategoryAlwaysAsSubCategory.ColorKey == SelectedColor.Key.ToString() &&
                editedCategoryAlwaysAsSubCategory.IconKey == SelectedIcon.Key.ToString() &&
                editedCategoryAlwaysAsSubCategory.Name == NameValue.Text &&
                !string.IsNullOrEmpty(NameValue.Text) &&
                editedCategoryAlwaysAsSubCategory.VisibleInExpenses == VisibleInExpensesToggleButton.IsOn &&
                editedCategoryAlwaysAsSubCategory.VisibleInIncomes == VisibleInIncomesToggleButton.IsOn;
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

        private void ColorBaseList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetPrimaryButtonEnabled();
        }
    }
}
