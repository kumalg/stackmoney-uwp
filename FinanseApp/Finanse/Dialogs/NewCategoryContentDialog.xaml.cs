using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Dialogs {

    public sealed partial class NewCategoryContentDialog : ContentDialog, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<KeyValuePair<object, object>> colorBase;
        private List<KeyValuePair<object, object>> ColorBase {
            get {
                if (colorBase == null) {
                    colorBase = ((ResourceDictionary)Application.Current.Resources["ColorBase"]).ToList();
                }
                return colorBase;
            }
        }
        private SolidColorBrush Color {
            get {
                return (SolidColorBrush)SelectedColor.Value;
            }
        }

        private string ColorKey {
            get {
                return SelectedColor.Key.ToString();
            }
        }

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
        private List<KeyValuePair<object, object>> IconBase {
            get {
                if (iconBase == null) {
                    iconBase = ((ResourceDictionary)Application.Current.Resources["IconBase"]).ToList();
                }
                return iconBase;
            }
        }
        private FontIcon Icon {
            get {
                return (FontIcon)SelectedIcon.Value;
            }
        }

        private string IconKey {
            get {
                return SelectedIcon.Key.ToString();
            }
        }

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

        private List<Category> Categories = Dal.getAllCategories();

        private Category newCategoryItem = new Category();
        public Category NewCategoryItem {
            get {
                return newCategoryItem;
            }
        }
        private Category editedCategoryItem;
        private SubCategory editedCategoryAlwaysAsSubCategory;

        private List<ComboBoxItem> categoriesInComboBox;
        private List<ComboBoxItem> CategoriesInComboBox {
            get {
                if (categoriesInComboBox == null) {
                    categoriesInComboBox = new List<ComboBoxItem>();
                    categoriesInComboBox.Add(new ComboBoxItem {
                        Content = "Brak",
                        Tag = -1,
                    });
                    foreach (Category categories_ComboBox in Categories) {
                        categoriesInComboBox.Add(new ComboBoxItem {
                            Content = categories_ComboBox.Name,
                            Tag = categories_ComboBox.Id
                        });
                    }
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
            
            newCategoryItem = new Category(editedCategoryItem);
        }

        public NewCategoryContentDialog(int BossCategoryId) {
            this.InitializeComponent();
            Title = "Nowa kategoria";

            Category bossCategory = Dal.getCategoryById(BossCategoryId);
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



        private object SelectedCategory {
            get {
                return CategoriesInComboBox.FirstOrDefault(i => ((int)i.Tag).Equals(BossCategoryId));
            }
        }

        private void SetPrimaryButtonEnabled() {
            IsPrimaryButtonEnabled = editedCategoryAlwaysAsSubCategory == null ? !string.IsNullOrEmpty(NameValue.Text) : !isNewOperationTheSame();
        }

        private bool isNewOperationTheSame() {
           
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

            newCategoryItem.Name = NameValue.Text;
            newCategoryItem.ColorKey = SelectedColor.Key.ToString();
            newCategoryItem.IconKey = SelectedIcon.Key.ToString();
            newCategoryItem.VisibleInExpenses = VisibleInExpensesToggleButton.IsOn;
            newCategoryItem.VisibleInIncomes = VisibleInIncomesToggleButton.IsOn;

            if (CategoryValue.SelectedIndex != -1) {
                SubCategory op = new SubCategory(newCategoryItem);
                op.BossCategoryId = BossCategoryId;

                newCategoryItem = op;
            }
        }
        
        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetPrimaryButtonEnabled();
        }

        private void setVisibleInExpensesAndIncomesToNewCategory() {
            newCategoryItem.VisibleInExpenses = VisibleInExpensesToggleButton.IsOn;
            newCategoryItem.VisibleInIncomes = VisibleInIncomesToggleButton.IsOn;
        }

        private void VisibleInExpensesToggleButton_Toggled(object sender, RoutedEventArgs e) {
            if (VisibleInIncomesToggleButton != null)
                if (!VisibleInIncomesToggleButton.IsEnabled)
                    VisibleInExpensesToggleButton.IsOn = true;

            if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInIncomesToggleButton.IsOn = true;

            setVisibleInExpensesAndIncomesToNewCategory();
            SetPrimaryButtonEnabled();
        }

        private void VisibleInIncomesToggleButton_Toggled(object sender, RoutedEventArgs e) {
            if (!VisibleInExpensesToggleButton.IsEnabled)
                VisibleInIncomesToggleButton.IsOn = true;

            else if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInExpensesToggleButton.IsOn = true;

            setVisibleInExpensesAndIncomesToNewCategory();
            SetPrimaryButtonEnabled();
        }

        private void CategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (((ComboBox)sender).SelectedIndex == 0)
                CategoryValue.SelectedIndex = -1;

            BossCategoryId = (CategoryValue.SelectedIndex == -1) ? -1 : (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;
            setExpenseAndIncomeToggleButtonsEnabling();

            SetPrimaryButtonEnabled();
        }

        private void setExpenseAndIncomeToggleButtonsEnabling() {
            Category bossCategory = Dal.getCategoryById(BossCategoryId);

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
