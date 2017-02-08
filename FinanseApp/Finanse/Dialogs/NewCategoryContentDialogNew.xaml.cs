using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Dialogs {

    public sealed partial class NewCategoryContentDialogNew : ContentDialog, INotifyPropertyChanged {

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

        private List<OperationCategory> OperationCategories = Dal.getAllCategories();

        private OperationCategory newOperationCategoryItem = new OperationCategory();
        public OperationCategory NewOperationCategoryItem {
            get {
                return newOperationCategoryItem;
            }
        }
        private OperationCategory editedOperationCategoryItem;
        private OperationSubCategory editedCategiryAlwaysAsSubCategory;

        private List<ComboBoxItem> operationCategoriesInComboBox;
        private List<ComboBoxItem> OperationCategoriesInComboBox {
            get {
                if (operationCategoriesInComboBox == null) {
                    operationCategoriesInComboBox = new List<ComboBoxItem>();
                    operationCategoriesInComboBox.Add(new ComboBoxItem {
                        Content = "Brak",
                        Tag = -1,
                    });
                    foreach (OperationCategory OperationCategories_ComboBox in OperationCategories) {
                        operationCategoriesInComboBox.Add(new ComboBoxItem {
                            Content = OperationCategories_ComboBox.Name,
                            Tag = OperationCategories_ComboBox.Id
                        });
                    }
                }

                return operationCategoriesInComboBox;
            }
        }



        public NewCategoryContentDialogNew(OperationCategory editedOperationCategoryItem) {
            this.InitializeComponent();
            this.editedOperationCategoryItem = new OperationCategory(editedOperationCategoryItem);
            this.editedCategiryAlwaysAsSubCategory = new OperationSubCategory(editedOperationCategoryItem);

            SelectedColor = ColorBase.FirstOrDefault(i => i.Key.Equals(editedOperationCategoryItem.ColorKey));
            SelectedIcon = IconBase.FirstOrDefault(i => i.Key.Equals(editedOperationCategoryItem.IconKey));

            BossCategoryId = editedCategiryAlwaysAsSubCategory.BossCategoryId;
            
            newOperationCategoryItem = new OperationCategory(editedOperationCategoryItem);
        }

        public NewCategoryContentDialogNew(int BossCategoryId) {
            this.InitializeComponent();

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
            this.BossCategoryId = BossCategoryId;
        }

        public NewCategoryContentDialogNew() {
            this.InitializeComponent();

            VisibleInExpensesToggleButton.IsOn = true;
            VisibleInIncomesToggleButton.IsOn = true;
        }



        private object SelectedCategory {
            get {
                return OperationCategoriesInComboBox.FirstOrDefault(i => ((int)i.Tag).Equals(BossCategoryId));
            }
        }

        private void SetPrimaryButtonEnabled() {
            IsPrimaryButtonEnabled = editedCategiryAlwaysAsSubCategory == null ? !string.IsNullOrEmpty(NameValue.Text) : !isNewOperationTheSame();
        }

        private bool isNewOperationTheSame() {
           
            return
                editedCategiryAlwaysAsSubCategory.BossCategoryId == BossCategoryId &&
                editedCategiryAlwaysAsSubCategory.ColorKey == SelectedColor.Key.ToString() &&
                editedCategiryAlwaysAsSubCategory.IconKey == SelectedIcon.Key.ToString() &&
                editedCategiryAlwaysAsSubCategory.Name == NameValue.Text &&
                !string.IsNullOrEmpty(NameValue.Text) &&
                editedCategiryAlwaysAsSubCategory.VisibleInExpenses == VisibleInExpensesToggleButton.IsOn &&
                editedCategiryAlwaysAsSubCategory.VisibleInIncomes == VisibleInIncomesToggleButton.IsOn;
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            newOperationCategoryItem.Name = NameValue.Text;
            newOperationCategoryItem.ColorKey = SelectedColor.Key.ToString();
            newOperationCategoryItem.IconKey = SelectedIcon.Key.ToString();
            newOperationCategoryItem.VisibleInExpenses = VisibleInExpensesToggleButton.IsOn;
            newOperationCategoryItem.VisibleInIncomes = VisibleInIncomesToggleButton.IsOn;

            if (CategoryValue.SelectedIndex != -1) {
                OperationSubCategory op = new OperationSubCategory(newOperationCategoryItem);
                op.BossCategoryId = BossCategoryId;

                newOperationCategoryItem = op;
            }
        }
        
        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetPrimaryButtonEnabled();
        }

        private void setVisibleInExpensesAndIncomesToNewCategory() {
            newOperationCategoryItem.VisibleInExpenses = VisibleInExpensesToggleButton.IsOn;
            newOperationCategoryItem.VisibleInIncomes = VisibleInIncomesToggleButton.IsOn;
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
            OperationCategory bossCategory = Dal.getOperationCategoryById(BossCategoryId);

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
