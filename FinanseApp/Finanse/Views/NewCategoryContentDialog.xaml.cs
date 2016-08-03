﻿using Finanse.Elements;
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

namespace Finanse.Views {

    public sealed partial class NewCategoryContentDialog : ContentDialog {

        ObservableCollection<OperationCategory> OperationCategories;

        SQLite.Net.SQLiteConnection conn;

        public OperationSubCategory newOperationSubCategoryItem;
        public OperationCategory newOperationCategoryItem;

        public OperationSubCategory editedOperationSubCategoryItem;
        public OperationCategory editedOperationCategoryItem;

        public OperationCategory editedCategory;
        public int editedId;
        public int editedBossCategoryId;

        public NewCategoryContentDialog(ObservableCollection<OperationCategory> OperationCategories, SQLite.Net.SQLiteConnection conn, OperationCategory editedCategory, int editedBossCategoryId) {

            this.InitializeComponent();

            this.OperationCategories = OperationCategories;
            this.conn = conn;
            this.editedCategory = editedCategory;
            editedId = editedCategory.Id;
            this.editedBossCategoryId = editedBossCategoryId;

            SetPrimaryButtonEnabled();

            /* DODAWANIE KATEGORII DO COMBOBOX'A */
            CategoryValue.Items.Add(new ComboBoxItem {
                Content = "Brak"
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
                CategoryCircle.Fill = GetSolidColorBrush(editedCategory.Color);

                VisibleInExpensesToggleButton.IsOn = editedCategory.VisibleInExpenses;
                VisibleInIncomesToggleButton.IsOn = editedCategory.VisibleInIncomes;

                if (editedBossCategoryId != -1) {
                    if (conn.Table<OperationCategory>().Any(i => i.Id == editedBossCategoryId)) {
                        OperationCategory catItem = conn.Table<OperationCategory>().Single(i => i.Id == editedBossCategoryId);
                        CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == catItem.Name);
                    }
                }
                SetPrimaryButtonEnabled();
            }
        }

        private void SetPrimaryButtonEnabled() {
            if (NameValue.Text != "") {
                IsPrimaryButtonEnabled = true;
            }
            else
                IsPrimaryButtonEnabled = false;
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            if (editedId == -1) {
                if (CategoryValue.SelectedIndex != -1) {
                    newOperationSubCategoryItem = new OperationSubCategory {
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    conn.Insert(newOperationSubCategoryItem);
                    OperationCategories.Single(x => x.Id == (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag).addSubCategory(newOperationSubCategoryItem);
                }

                else {
                    newOperationCategoryItem = new OperationCategory {
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    OperationCategories.Insert(0, newOperationCategoryItem);
                    conn.Insert(newOperationCategoryItem);
                }
            }
            else {

                if (editedBossCategoryId != -1 && CategoryValue.SelectedIndex != -1) {
                    // subkategoria która dalej jest subkategorią

                    editedOperationSubCategoryItem = new OperationSubCategory {
                        OperationCategoryId = editedCategory.Id,
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };
                    conn.Update(editedOperationSubCategoryItem);

                    if (editedBossCategoryId == (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag) {
                        ObservableCollection<OperationSubCategory> subCategories = OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedBossCategoryId))].subCategories;

                        subCategories[subCategories.IndexOf(subCategories.Single(c => c.OperationCategoryId == editedCategory.Id))] = editedOperationSubCategoryItem;

                        OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedBossCategoryId))].subCategories = subCategories;
                    }

                    else
                        RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId != -1 && CategoryValue.SelectedIndex == -1) {
                    // subkategoria która zostala kategorią
                    editedOperationSubCategoryItem = new OperationSubCategory {
                        OperationCategoryId = editedCategory.Id,
                    };
                    editedOperationCategoryItem = new OperationCategory {
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    conn.Delete(editedOperationSubCategoryItem);
                    conn.Insert(editedOperationCategoryItem);
                    RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId == -1 && CategoryValue.SelectedIndex != -1) {
                    // kategoria która została subkategorią
                    editedOperationSubCategoryItem = new OperationSubCategory {
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        BossCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    conn.Delete(editedCategory);
                    conn.Insert(editedOperationSubCategoryItem);
                    RefreshOperationCategoriesList();
                }
                else if (editedBossCategoryId == -1 && CategoryValue.SelectedIndex == -1) {
                    // kategoria która dalej jest kategorią

                    editedOperationCategoryItem = new OperationCategory {
                        Id = editedCategory.Id,
                        Name = NameValue.Text,
                        Color = ((SolidColorBrush)CategoryCircle.Fill).Color.ToString(),
                        Icon = CategoryIcon.Glyph,
                        VisibleInExpenses = VisibleInExpensesToggleButton.IsOn,
                        VisibleInIncomes = VisibleInIncomesToggleButton.IsOn
                    };

                    OperationCategories[OperationCategories.IndexOf(OperationCategories.Single(c => c.Id == editedCategory.Id))] = editedOperationCategoryItem;
                    conn.Update(editedOperationCategoryItem);
                }

            }
        }

        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e) {
            var button = sender as RadioButton;
            CategoryCircle.Fill = button.Background;
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
                foreach (var message in conn.Table<OperationCategory>()) {
                    if (message.Name == NamVal.Text) {
                        NameValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
                        break;
                    }
                    else {
                        NameValue.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                        SetPrimaryButtonEnabled();
                    }
                }
            }
            else {
                foreach (var message in conn.Table<OperationSubCategory>().Where(subCategory => subCategory.BossCategoryId == selectedCategoryId)) {

                    if (message.Name == NamVal.Text) {
                        NameValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"];
                        break;
                    }
                    else {
                        NameValue.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                        SetPrimaryButtonEnabled();
                    }
                }
            }
        }

        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

            SetPrimaryButtonEnabled();

            if (CategoryValue.SelectedIndex == -1) {
                foreach (var message in conn.Table<OperationCategory>()) {
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

                foreach (var message in conn.Table<OperationSubCategory>().Where(subCategory => subCategory.BossCategoryId == selectedCategoryId)) {
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
            if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInIncomesToggleButton.IsOn = true;
        }

        private void VisibleInIncomesToggleButton_Toggled(object sender, RoutedEventArgs e) {
            if (!VisibleInExpensesToggleButton.IsOn && !VisibleInIncomesToggleButton.IsOn)
                VisibleInExpensesToggleButton.IsOn = true;
        }

        private void CategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (CategoryValue.SelectedIndex == 0)
                CategoryValue.SelectedIndex--;
        }
        private SolidColorBrush GetSolidColorBrush(string hex) {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }

        private void RefreshOperationCategoriesList() {
            OperationCategories.Clear();
            foreach (var message in conn.Table<OperationCategory>().OrderBy(category => category.Name)) {
                OperationCategory operationCategoryItem = message;

                foreach (var submessage in conn.Table<OperationSubCategory>().OrderByDescending(subCategory => subCategory.Name)) {
                    if (submessage.BossCategoryId == message.Id) {
                        operationCategoryItem.addSubCategory(submessage);
                    }
                }
                OperationCategories.Add(operationCategoryItem);
            }
        }
    }
}
