using Finanse.DataAccessLayer;
using Finanse.Elements;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Dialogs {

    public sealed partial class NewCategoryContentDialog : ContentDialog {

        ObservableCollection<OperationCategory> OperationCategories;

        public OperationSubCategory newOperationSubCategoryItem;
        public OperationCategory newOperationCategoryItem;

        public OperationSubCategory editedOperationSubCategoryItem;
        public OperationCategory editedOperationCategoryItem;

        public OperationCategory editedCategory;
        public int editedId;
        public int editedBossCategoryId;

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

                CategoryIcon.Glyph = editedCategory.Icon;
                CategoryCircle.Fill = Functions.GetSolidColorBrush(editedCategory.Color);

                VisibleInExpensesToggleButton.IsOn = editedCategory.VisibleInExpenses;
                VisibleInIncomesToggleButton.IsOn = editedCategory.VisibleInIncomes;

                if (editedBossCategoryId != -1) {
                    if (Dal.GetAllCategories().Any(i => i.Id == editedBossCategoryId)) {
                        OperationCategory catItem = Dal.GetOperationCategoryById(editedBossCategoryId);
                        CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == catItem.Name);
                    }
                }
                SetPrimaryButtonEnabled();
            }
        }

        private void SetPrimaryButtonEnabled() {

            IsPrimaryButtonEnabled = (NameValue.Text != "");
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            if (editedId == -1) {
                if (CategoryValue.SelectedIndex != -1) {
                    newOperationSubCategoryItem = new OperationSubCategory {
                        Id = 0,
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        //Icon = Icon2Value.Text,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    Dal.SaveOperationSubCategory(newOperationSubCategoryItem);
                    OperationCategories.Single(x => x.Id == (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag).addSubCategory(newOperationSubCategoryItem);
                }

                else {
                    newOperationCategoryItem = new OperationCategory {
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        //Icon = Icon2Value.Text,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    OperationCategories.Insert(0, newOperationCategoryItem);
                    Dal.SaveOperationCategory(newOperationCategoryItem);
                }
            }
            else {

                if (editedBossCategoryId != -1 && CategoryValue.SelectedIndex != -1) {
                    // subkategoria która dalej jest subkategorią

                    editedOperationSubCategoryItem = new OperationSubCategory {
                        Id = editedCategory.Id,
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        //Icon = Icon2Value.Text,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };
                    Dal.SaveOperationSubCategory(editedOperationSubCategoryItem);

                    if (editedBossCategoryId == (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag) {
                        ObservableCollection<OperationSubCategory> subCategories = OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedBossCategoryId))].subCategories;

                        subCategories[subCategories.IndexOf(subCategories.Single(c => c.Id == editedCategory.Id))] = editedOperationSubCategoryItem;

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
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        //Icon = Icon2Value.Text,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    Dal.DeleteSubCategory(editedOperationSubCategoryItem);
                    Dal.SaveOperationCategory(editedOperationCategoryItem);

                    RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId == -1 && CategoryValue.SelectedIndex != -1) {
                    // kategoria która została subkategorią
                    editedOperationSubCategoryItem = new OperationSubCategory {
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        //Icon = Icon2Value.Text,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    Dal.DeleteCategory(editedCategory);
                    Dal.SaveOperationSubCategory(editedOperationSubCategoryItem);

                    RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId == -1 && CategoryValue.SelectedIndex == -1) {
                    // kategoria która dalej jest kategorią

                    editedOperationCategoryItem = new OperationCategory {
                        Id = editedCategory.Id,
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        //Icon = Icon2Value.Text,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedCategory.Id))] = editedOperationCategoryItem;
                    Dal.SaveOperationCategory(editedOperationCategoryItem);
                }

            }
        }

        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e) {
            var button = sender as RadioButton;
            //CategoryCircle.Fill = button.Background;
            CategoryCircle.Stroke = button.Background;
            CategoryIcon.Foreground = button.Background;
        }

        private void RadioButtonIcon_Checked(object sender, RoutedEventArgs e) {
            var button = sender as RadioButton;
            CategoryIcon.Glyph = button.Content.ToString();
            CategoryIcon.FontFamily = button.FontFamily;
        }

        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {

            TextBox NamVal = sender as TextBox;

            int selectedCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;

            if (CategoryValue.SelectedIndex == -1) {

                if (Dal.GetAllCategories().Any(item => item.Name == NamVal.Text))
                    NameValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
                else {
                    NameValue.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                    SetPrimaryButtonEnabled();
                }
            }
            else {
                if (Dal.GetOperationSubCategoriesByBossId(selectedCategoryId).Any(item => item.Name == NamVal.Text)) {
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
                foreach (var message in Dal.GetAllCategories()) {
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

                foreach (var message in Dal.GetOperationSubCategoriesByBossId(selectedCategoryId)) {
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
                OperationCategory item = Dal.GetOperationCategoryById((int)((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag);

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
            foreach (var message in Dal.GetAllCategories()) {

                message.subCategories = new ObservableCollection<OperationSubCategory>(Dal.GetOperationSubCategoriesByBossId(message.Id));

                OperationCategories.Add(message);
            }
        }
    }
}
