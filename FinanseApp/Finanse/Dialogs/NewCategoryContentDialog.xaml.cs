using Finanse.DataAccessLayer;
using Finanse.Elements;
using Finanse.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Dialogs {

    public sealed partial class NewCategoryContentDialog : ContentDialog {

        private string colorKey = string.Empty;
        private string iconKey = string.Empty;

        ObservableCollection<OperationCategory> OperationCategories;

        public OperationSubCategory newOperationSubCategoryItem;
        public OperationCategory newOperationCategoryItem;

        public OperationSubCategory editedOperationSubCategoryItem;
        public OperationCategory editedOperationCategoryItem;

        public OperationCategory editedCategory;
        public int editedId;
        public int editedBossCategoryId;

        public NewCategoryContentDialog(ObservableCollection<OperationCategory> OperationCategories, OperationCategory BossCategory) {

            this.InitializeComponent();

            this.OperationCategories = OperationCategories;
            editedId = -1;

            SetPrimaryButtonEnabled();

            /* DODAWANIE KATEGORII DO COMBOBOX'A */
            CategoryValue.Items.Add(new ComboBoxItem {
                Content = "Brak",
                Tag = -1,
            });
            foreach (OperationCategory OperationCategories_ComboBox in OperationCategories) {

                CategoryValue.Items.Add(new ComboBoxItem {
                    Content = OperationCategories_ComboBox.Name,
                    Tag = OperationCategories_ComboBox.Id
                });
            }

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == BossCategory.Name);
            ColorBaseList.ItemsSource = ((ResourceDictionary)Application.Current.Resources["ColorBase"]).OrderByDescending(i=>i.Key.ToString());
        }

        public NewCategoryContentDialog(ObservableCollection<OperationCategory> OperationCategories, OperationCategory editedCategory, int editedBossCategoryId) {

            this.InitializeComponent();

            this.OperationCategories = OperationCategories;
            this.editedCategory = editedCategory;
            editedId = editedCategory.Id;
            this.editedBossCategoryId = editedBossCategoryId;

            SetPrimaryButtonEnabled();

            /* DODAWANIE KATEGORII DO COMBOBOX'A */
            CategoryValue.Items.Add(new ComboBoxItem {
                Content = "Brak",
                Tag = -1,
            });
            foreach (OperationCategory OperationCategories_ComboBox in OperationCategories) {

                CategoryValue.Items.Add(new ComboBoxItem {
                    Content = OperationCategories_ComboBox.Name,
                    Tag = OperationCategories_ComboBox.Id
                });
            }

            if (editedCategory.Id != -1) {
                Title = "Edycja kategorii";
                PrimaryButtonText = "Zapisz";

                NameValue.Text = editedCategory.Name;

                CategoryIcon.Glyph = editedCategory.Icon.Glyph;//.ToString();
                iconKey = editedCategory.IconKey;
                CategoryIcon.Color = editedCategory.Color;//Functions.GetSolidColorBrush(editedCategory.Color);
                colorKey = editedCategory.ColorKey;

                VisibleInExpensesToggleButton.IsOn = editedCategory.VisibleInExpenses;
                VisibleInIncomesToggleButton.IsOn = editedCategory.VisibleInIncomes;

                if (editedBossCategoryId != -1) {
                    if (Dal.getAllCategories().Any(i => i.Id == editedBossCategoryId)) {
                        OperationCategory catItem = Dal.getOperationCategoryById(editedBossCategoryId);
                        CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == catItem.Name);
                    }
                }
                SetPrimaryButtonEnabled();
            }
            ColorBaseList.ItemsSource = ((ResourceDictionary)Application.Current.Resources["ColorBase"]).OrderBy(i => i.Key.ToString());
        }

        private void SetPrimaryButtonEnabled() {

            IsPrimaryButtonEnabled = (NameValue.Text != "");
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            if (editedId == -1) {
                if (CategoryValue.SelectedIndex != -1) {
                    int bossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;

                    newOperationSubCategoryItem = new OperationSubCategory {
                        Id = 0,
                        Name = NameValue.Text,
                        ColorKey = colorKey,
                        IconKey = iconKey,
                        BossCategoryId = bossCategoryId,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    Dal.saveOperationSubCategory(newOperationSubCategoryItem);
                    OperationCategories.Single(x => x.Id == bossCategoryId).addSubCategory(newOperationSubCategoryItem);
                }

                else {
                    newOperationCategoryItem = new OperationCategory {
                        Name = NameValue.Text,
                        ColorKey = colorKey,//((RadioButton)((GridViewItem)ColorBaseList.SelectedItem).Content).Content.ToString(), //CategoryIcon.Color.Color.ToString(),
                        IconKey = iconKey,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    OperationCategories.Insert(0, newOperationCategoryItem);
                    Dal.saveOperationCategory(newOperationCategoryItem);
                }
            }
            else {

                if (editedBossCategoryId != -1 && CategoryValue.SelectedIndex != -1) {
                    // subkategoria która dalej jest subkategorią

                    editedOperationSubCategoryItem = new OperationSubCategory {
                        Id = editedCategory.Id,
                        Name = NameValue.Text,
                        ColorKey = colorKey,
                        IconKey = iconKey,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };
                    Dal.saveOperationSubCategory(editedOperationSubCategoryItem);

                    if (editedBossCategoryId == (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag) {
                        ObservableCollection<OperationSubCategory> subCategories = OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedBossCategoryId))].subCategories;

                        subCategories[subCategories.IndexOf(subCategories.Single(c => c.Id == editedCategory.Id))] = editedOperationSubCategoryItem;///$$$$$$$$$$4

                        OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedBossCategoryId))].subCategories = subCategories;
                    }

                    else
                        RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId != -1 && CategoryValue.SelectedIndex == -1) {
                    // subkategoria która zostala kategorią
                    editedOperationSubCategoryItem = new OperationSubCategory {
                        Id = editedCategory.Id,
                    };
                    editedOperationCategoryItem = new OperationCategory {
                        Name = NameValue.Text,
                        ColorKey = colorKey,
                        IconKey = iconKey,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    Dal.deleteSubCategory(editedOperationSubCategoryItem);
                    Dal.saveOperationCategory(editedOperationCategoryItem);

                    RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId == -1 && CategoryValue.SelectedIndex != -1) {
                    // kategoria która została subkategorią
                    editedOperationSubCategoryItem = new OperationSubCategory {
                        Name = NameValue.Text,
                        ColorKey = colorKey,
                        IconKey = iconKey,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    Dal.deleteCategory(editedCategory);
                    Dal.saveOperationSubCategory(editedOperationSubCategoryItem);

                    RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId == -1 && CategoryValue.SelectedIndex == -1) {
                    // kategoria która dalej jest kategorią

                    editedOperationCategoryItem = new OperationCategory {
                        Id = editedCategory.Id,
                        Name = NameValue.Text,
                        ColorKey = colorKey,
                        IconKey = iconKey,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedCategory.Id))] = editedOperationCategoryItem;
                    Dal.saveOperationCategory(editedOperationCategoryItem);
                }

            }
        }

        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e) {
            var button = sender as RadioButton;
            colorKey = button.Content.ToString();
            CategoryIcon.Color = (SolidColorBrush)button.Background;
        }

        private void RadioButtonIcon_Checked(object sender, RoutedEventArgs e) {
            var button = sender as RadioButton;
            iconKey = button.Tag.ToString();
            CategoryIcon.Glyph = button.Content.ToString();
        }

        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {

            TextBox NamVal = sender as TextBox;

            int selectedCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;

            if (CategoryValue.SelectedIndex == -1) {

                if (Dal.getAllCategories().Any(item => item.Name == NamVal.Text))
                    NameValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
                else {
                    NameValue.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                    SetPrimaryButtonEnabled();
                }
            }
            else {
                if (Dal.getOperationSubCategoriesByBossId(selectedCategoryId).Any(item => item.Name == NamVal.Text)) {
                    NameValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
                }
                else {
                    NameValue.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                    SetPrimaryButtonEnabled();
                }
            }
        }

        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

            SetPrimaryButtonEnabled();

            if (CategoryValue.SelectedIndex == -1) {
                foreach (var message in Dal.getAllCategories()) {
                    if (message.Name.ToLower() == NameValue.Text.ToLower()) {
                        NameValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
                        IsPrimaryButtonEnabled = false;
                        break;
                    }
                    else {
                        NameValue.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                        SetPrimaryButtonEnabled();
                    }
                }
            }
            else {
                int selectedCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;

                foreach (var message in Dal.getOperationSubCategoriesByBossId(selectedCategoryId)) {
                    if (message.Name.ToLower() == NameValue.Text.ToLower()) {
                        NameValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
                        IsPrimaryButtonEnabled = false;
                        break;
                    }
                    else {
                        NameValue.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                        SetPrimaryButtonEnabled();
                    }
                }
            }
        }

        private void VisibleInExpensesToggleButton_Toggled(object sender, RoutedEventArgs e) {
            if(VisibleInIncomesToggleButton != null)
                if (!VisibleInIncomesToggleButton.IsEnabled)
                    VisibleInExpensesToggleButton.IsOn = true;

            if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInIncomesToggleButton.IsOn = true;
        }

        private void VisibleInIncomesToggleButton_Toggled(object sender, RoutedEventArgs e) {
            if (!VisibleInExpensesToggleButton.IsEnabled)
                VisibleInIncomesToggleButton.IsOn = true;

            else if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInExpensesToggleButton.IsOn = true;
        }

        private void CategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (CategoryValue.SelectedIndex == 0) {
                CategoryValue.SelectedIndex--;

                VisibleInExpensesToggleButton.IsEnabled = true;
                VisibleInExpensesToggleButton.IsOn = true;

                VisibleInIncomesToggleButton.IsEnabled = true;
                VisibleInIncomesToggleButton.IsOn = true;
            }
            else {
                OperationCategory item = Dal.getOperationCategoryById((int)((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag);

                VisibleInExpensesToggleButton.IsEnabled = item.VisibleInExpenses;
                if (item.VisibleInExpenses) {
                    VisibleInExpensesToggleButton.IsEnabled = true;
                    VisibleInExpensesToggleButton.IsOn = true;
                }
                else {
                    VisibleInExpensesToggleButton.IsEnabled = false;
                    VisibleInExpensesToggleButton.IsOn = false;
                }

                if (item.VisibleInIncomes) {
                    VisibleInIncomesToggleButton.IsEnabled = true;
                    VisibleInIncomesToggleButton.IsOn = true;
                }
                else {
                    VisibleInIncomesToggleButton.IsEnabled = false;
                    VisibleInIncomesToggleButton.IsOn = false;
                }
            }
        }

        private void RefreshOperationCategoriesList() {
            OperationCategories.Clear();
            foreach (var message in Dal.getAllCategories()) {

                message.subCategories = new ObservableCollection<OperationSubCategory>(Dal.getOperationSubCategoriesByBossId(message.Id));

                OperationCategories.Add(message);
            }
        }
    }
}
