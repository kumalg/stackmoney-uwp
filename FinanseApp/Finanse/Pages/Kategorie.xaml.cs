using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using Finanse.Models.Categories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Finanse.Pages {

    public sealed partial class Kategorie : Page, INotifyPropertyChanged {

        private ObservableCollection<CategoryWithSubCategories> operationCategories;
        private ObservableCollection<CategoryWithSubCategories> OperationCategories {
            get {
                return operationCategories;
            }
            set {
                operationCategories = value;
                RaisePropertyChanged("OperationCategories");
            }
        }

        private ObservableCollection<CategoryWithSubCategories> operationCategoriesTest;
        private ObservableCollection<CategoryWithSubCategories> OperationCategoriesTest {
            get {
                return operationCategoriesTest;
            }
            set {
                operationCategoriesTest = value;
                RaisePropertyChanged("OperationCategoriesTest");
            }
        }



        private ObservableCollection<CategoryWithSubCategories> expenseCategories;
        private ObservableCollection<CategoryWithSubCategories> ExpenseCategories {
            get {
                if (expenseCategories == null)
                    expenseCategories = new ObservableCollection<CategoryWithSubCategories>(Dal.getOperationCategoriesWithSubCategoriesInExpenses());

                return expenseCategories;
            }
            set {
                expenseCategories = value;
                RaisePropertyChanged("ExpenseCategories");
            }
        }

        private ObservableCollection<CategoryWithSubCategories> incomeCategories;
        private ObservableCollection<CategoryWithSubCategories> IncomeCategories {
            get {
                if (incomeCategories == null)
                    incomeCategories = new ObservableCollection<CategoryWithSubCategories>(Dal.getOperationCategoriesWithSubCategoriesInIncomes());

                return incomeCategories;
            }
            set {
                incomeCategories = value;
                RaisePropertyChanged("IncomeCategories");
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        public Kategorie() {
            this.InitializeComponent();
            OperationCategories = ExpenseCategories;
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            showSubCategoryFlyout(sender);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
            showSubCategoryFlyout(sender);
        }

        private void showSubCategoryFlyout(object sender) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void EditCategory_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            CategoryWithSubCategories thisCategorys = (CategoryWithSubCategories)datacontext;
            var ContentDialogItem = new NewCategoryContentDialog(thisCategorys.Category);
            var result = await ContentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                OperationCategory cat = ContentDialogItem.NewOperationCategoryItem;

                if (cat is OperationSubCategory)
                    updateCategory(thisCategorys, cat as OperationSubCategory);
                else
                    updateCategory(thisCategorys, cat);
            }
        }

        private void updateCategory(CategoryWithSubCategories categoryWithSubCategories, OperationCategory category) {

            CategoryWithSubCategories newCategoryWithSubCategorie = new CategoryWithSubCategories {
                Category = category,
                SubCategories = categoryWithSubCategories.SubCategories
            };

            int indexInExpenses = -1, indexInIncomes = -1;

            if (categoryWithSubCategories.Category.VisibleInExpenses)
                indexInExpenses = ExpenseCategories.IndexOf(categoryWithSubCategories);
            if (categoryWithSubCategories.Category.VisibleInIncomes)
                indexInIncomes = IncomeCategories.IndexOf(categoryWithSubCategories);

            //   int index = OperationCategoriesTest.IndexOf(categoryWithSubCategories);
            // OperationCategoriesTest.Remove(categoryWithSubCategories);
            tryRemoveCategoryWithSubCategoriesInList(categoryWithSubCategories);
            tryInsertCategoryWithSubCategoriesInList(indexInExpenses, indexInIncomes, newCategoryWithSubCategorie);

            /*
            if (isCategoryVisibleInCurrentView(category))
                OperationCategoriesTest.Insert(index, newCategoryWithSubCategorie);
                */

            Dal.updateOperationCategory(category);
        }

        private void updateCategory(CategoryWithSubCategories categoryWithSubCategories, OperationSubCategory subCategory) {
            /*
            OperationCategoriesTest
                .Remove(categoryWithSubCategories);
                */
            tryRemoveCategoryWithSubCategoriesInList(categoryWithSubCategories);
            tryAddSubCategoryInList(subCategory);

            Dal.deleteCategoryWithSubCategories(categoryWithSubCategories.Category.Id);
            Dal.addOperationSubCategory(subCategory);
        }

        private async void EditSubCategory_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            OperationSubCategory thisSubCategory = (OperationSubCategory)datacontext;
            var ContentDialogItem = new NewCategoryContentDialog(thisSubCategory);
            var result = await ContentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                OperationCategory cat = ContentDialogItem.NewOperationCategoryItem;

                if (cat is OperationSubCategory)
                    updateSubCategory(thisSubCategory, cat as OperationSubCategory);
                else
                    updateSubCategory(thisSubCategory, cat);
            }
        }

        private void updateSubCategory(OperationSubCategory oldSubCategory, OperationCategory category) {
            /*
            OperationCategoriesTest
                .FirstOrDefault(item => item.Category.Id == oldSubCategory.BossCategoryId)
                .SubCategories
                .Remove(oldSubCategory);
                */
            tryRemoveSubCategoryInList(oldSubCategory);
            tryAddCategoryInList(category);

            Dal.deleteSubCategory(category.Id);
            Dal.addOperationCategory(category);
        }

        private void updateSubCategory(OperationSubCategory oldSubCategory, OperationSubCategory subCategory) {
            tryRemoveSubCategoryInList(oldSubCategory);
            tryAddSubCategoryInList(subCategory);
            Dal.updateOperationSubCategory(subCategory);
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            OperationCategory category = ((CategoryWithSubCategories)datacontext).Category;

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć kategorię i wszystkie jej podkategorie?");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                //OperationCategoriesTest.Remove(OperationCategoriesTest.FirstOrDefault(item => item.Category == category));
                tryRemoveCategoryInList(category);
                Dal.deleteCategoryWithSubCategories(category.Id);
            }
        }

        private async void DeleteSubCategory_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            OperationSubCategory subCategory = (OperationSubCategory)datacontext;

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć podkategorię?");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                /*
                OperationCategoriesTest
                    .FirstOrDefault(item => item.Category.Id == subCategory.BossCategoryId)
                        .SubCategories.Remove(subCategory);
                        */
                tryRemoveSubCategoryInList(subCategory);
                Dal.deleteSubCategory(subCategory.Id);
            }
        }

        private async void AddSubCat_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            CategoryWithSubCategories categoryWithSubCategories = (CategoryWithSubCategories)datacontext;

            NewCategoryContentDialog ContentDialogItem = new NewCategoryContentDialog(categoryWithSubCategories.Category.Id);
            ContentDialogResult result = await ContentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary)
                addNewCategoryOrSubCategoryToListAndSQL(ContentDialogItem.NewOperationCategoryItem);
        }

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            NewCategoryContentDialog ContentDialogItem = new NewCategoryContentDialog();
            ContentDialogResult result = await ContentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary)
                addNewCategoryOrSubCategoryToListAndSQL(ContentDialogItem.NewOperationCategoryItem);
        }

        private void addNewCategoryOrSubCategoryToListAndSQL(OperationCategory newCategoryOrSubCategory) {
            if (newCategoryOrSubCategory is OperationSubCategory) {
                tryAddSubCategoryInList((OperationSubCategory)newCategoryOrSubCategory);
                Dal.addOperationSubCategory((OperationSubCategory)newCategoryOrSubCategory);
            }
            else {
                tryAddCategoryInList(newCategoryOrSubCategory);
                Dal.addOperationCategory(newCategoryOrSubCategory);
            }
        }

        private void tryRemoveSubCategoryInList(OperationSubCategory subCategory) {
            if (subCategory.VisibleInExpenses)
                ExpenseCategories
                    .FirstOrDefault(item => item.Category.Id == subCategory.BossCategoryId)
                    .SubCategories
                    .Remove(subCategory);

            if (subCategory.VisibleInIncomes)
                IncomeCategories
                    .FirstOrDefault(item => item.Category.Id == subCategory.BossCategoryId)
                    .SubCategories
                    .Remove(subCategory);
        }

        private void tryAddSubCategoryInList(OperationSubCategory subCategory) {
            if (subCategory.VisibleInExpenses)
                ExpenseCategories
                    .FirstOrDefault(item => item.Category.Id == subCategory.BossCategoryId)
                    .SubCategories.Insert(0, subCategory);

            if (subCategory.VisibleInIncomes)
                IncomeCategories
                    .FirstOrDefault(item => item.Category.Id == subCategory.BossCategoryId)
                    .SubCategories.Insert(0, subCategory);
        }

        private void tryRemoveCategoryWithSubCategoriesInList(CategoryWithSubCategories categoryWithSubCategories) {
            if (categoryWithSubCategories.Category.VisibleInExpenses)
                ExpenseCategories.Remove(categoryWithSubCategories);

            if (categoryWithSubCategories.Category.VisibleInIncomes)
                IncomeCategories.Remove(categoryWithSubCategories);
        }

        private void tryRemoveCategoryInList(OperationCategory operationCategory) {
            if (operationCategory.VisibleInExpenses)
                ExpenseCategories.Remove(ExpenseCategories.FirstOrDefault(i => i.Category.Id == operationCategory.Id));

            if (operationCategory.VisibleInIncomes)
                IncomeCategories.Remove(ExpenseCategories.FirstOrDefault(i => i.Category.Id == operationCategory.Id));
        }

        private void tryInsertCategoryWithSubCategoriesInList(int indexInExpenses, int indexInIncomes, CategoryWithSubCategories categoryWithSubCategories) {
            if (categoryWithSubCategories.Category.VisibleInExpenses) {
                if (indexInExpenses == -1)
                    indexInExpenses = 0;
                ExpenseCategories.Insert(indexInExpenses, categoryWithSubCategories);
            }

            if (categoryWithSubCategories.Category.VisibleInIncomes) {
                if (indexInIncomes == -1)
                    indexInIncomes = 0;
                IncomeCategories.Insert(indexInIncomes, categoryWithSubCategories);
            }
        }

        private void tryAddCategoryInList(OperationCategory category) {
            if (category.VisibleInExpenses)
                ExpenseCategories.Insert(0, new CategoryWithSubCategories {
                    Category = category
                });

            if (category.VisibleInIncomes)
                IncomeCategories.Insert(0, new CategoryWithSubCategories {
                    Category = category
                });
        }

        private bool isCategoryVisibleInCurrentView(OperationCategory category) {
            return ((bool)ExpenseRadioButton.IsChecked && category.VisibleInExpenses) ||
                   ((bool)IncomeRadioButton.IsChecked && category.VisibleInIncomes);
        }


        private async void RadioButton_Checked(object sender, RoutedEventArgs e) {
            await Task.Delay(5);
            OperationCategories = ExpenseCategories;
        }

        private async void RadioButton_Checked_1(object sender, RoutedEventArgs e) {
            await Task.Delay(5);
            OperationCategories = IncomeCategories;
        }

        private void SubOperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListView listView = sender as ListView;
            listView.SelectedIndex = -1;
        }
        
        private void ExpandPanel_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            showFlyoutBase(sender);
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e) {
            showFlyoutBase(sender);
        }

        private void showFlyoutBase(object sender) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }
    }
}
