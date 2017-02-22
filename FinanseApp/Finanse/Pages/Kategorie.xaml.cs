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

        private ObservableCollection<CategoryWithSubCategories> categories;
        private ObservableCollection<CategoryWithSubCategories> Categories {
            get {
                return categories;
            }
            set {
                categories = value;
                RaisePropertyChanged("Categories");
            }
        }

        private ObservableCollection<CategoryWithSubCategories> categoriesTest;
        private ObservableCollection<CategoryWithSubCategories> CategoriesTest {
            get {
                return categoriesTest;
            }
            set {
                categoriesTest = value;
                RaisePropertyChanged("CategoriesTest");
            }
        }



        private ObservableCollection<CategoryWithSubCategories> expenseCategories;
        private ObservableCollection<CategoryWithSubCategories> ExpenseCategories {
            get {
                if (expenseCategories == null)
                    expenseCategories = new ObservableCollection<CategoryWithSubCategories>(Dal.getCategoriesWithSubCategoriesInExpenses());

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
                    incomeCategories = new ObservableCollection<CategoryWithSubCategories>(Dal.getCategoriesWithSubCategoriesInIncomes());

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
            Categories = ExpenseCategories;
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
                Category cat = ContentDialogItem.NewCategoryItem;

                if (cat is SubCategory)
                    updateCategory(thisCategorys, cat as SubCategory);
                else
                    updateCategory(thisCategorys, cat);
            }
        }

        private void updateCategory(CategoryWithSubCategories categoryWithSubCategories, Category category) {

            CategoryWithSubCategories newCategoryWithSubCategorie = new CategoryWithSubCategories {
                Category = category,
                SubCategories = categoryWithSubCategories.SubCategories
            };

            int indexInExpenses = -1, indexInIncomes = -1;

            if (categoryWithSubCategories.Category.VisibleInExpenses)
                indexInExpenses = ExpenseCategories.IndexOf(categoryWithSubCategories);
            if (categoryWithSubCategories.Category.VisibleInIncomes)
                indexInIncomes = IncomeCategories.IndexOf(categoryWithSubCategories);

            //   int index = CategoriesTest.IndexOf(categoryWithSubCategories);
            // CategoriesTest.Remove(categoryWithSubCategories);
            tryRemoveCategoryWithSubCategoriesInList(categoryWithSubCategories);
            tryInsertCategoryWithSubCategoriesInList(indexInExpenses, indexInIncomes, newCategoryWithSubCategorie);

            /*
            if (isCategoryVisibleInCurrentView(category))
                CategoriesTest.Insert(index, newCategoryWithSubCategorie);
                */

            Dal.updateCategory(category);
        }

        private void updateCategory(CategoryWithSubCategories categoryWithSubCategories, SubCategory subCategory) {
            /*
            CategoriesTest
                .Remove(categoryWithSubCategories);
                */
            tryRemoveCategoryWithSubCategoriesInList(categoryWithSubCategories);
            tryAddSubCategoryInList(subCategory);

            Dal.deleteCategoryWithSubCategories(categoryWithSubCategories.Category.Id);
            Dal.addSubCategory(subCategory);
        }

        private async void EditSubCategory_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            SubCategory thisSubCategory = (SubCategory)datacontext;
            var ContentDialogItem = new NewCategoryContentDialog(thisSubCategory);
            var result = await ContentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                Category cat = ContentDialogItem.NewCategoryItem;

                if (cat is SubCategory)
                    updateSubCategory(thisSubCategory, cat as SubCategory);
                else
                    updateSubCategory(thisSubCategory, cat);
            }
        }

        private void updateSubCategory(SubCategory oldSubCategory, Category category) {
            /*
            CategoriesTest
                .FirstOrDefault(item => item.Category.Id == oldSubCategory.BossCategoryId)
                .SubCategories
                .Remove(oldSubCategory);
                */
            tryRemoveSubCategoryInList(oldSubCategory);
            tryAddCategoryInList(category);

            Dal.deleteSubCategory(category.Id);
            Dal.addCategory(category);
        }

        private void updateSubCategory(SubCategory oldSubCategory, SubCategory subCategory) {
            tryRemoveSubCategoryInList(oldSubCategory);
            tryAddSubCategoryInList(subCategory);
            Dal.updateSubCategory(subCategory);
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Category category = ((CategoryWithSubCategories)datacontext).Category;

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć kategorię i wszystkie jej podkategorie?");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                //CategoriesTest.Remove(OperationCategoriesTest.FirstOrDefault(item => item.Category == category));
                tryRemoveCategoryInList(category);
                Dal.deleteCategoryWithSubCategories(category.Id);
            }
        }

        private async void DeleteSubCategory_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            SubCategory subCategory = (SubCategory)datacontext;

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć podkategorię?");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                /*
                CategoriesTest
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
                addNewCategoryOrSubCategoryToListAndSQL(ContentDialogItem.NewCategoryItem);
        }

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            NewCategoryContentDialog ContentDialogItem = new NewCategoryContentDialog();
            ContentDialogResult result = await ContentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary)
                addNewCategoryOrSubCategoryToListAndSQL(ContentDialogItem.NewCategoryItem);
        }

        private void addNewCategoryOrSubCategoryToListAndSQL(Category newCategoryOrSubCategory) {
            if (newCategoryOrSubCategory is SubCategory) {
                tryAddSubCategoryInList((SubCategory)newCategoryOrSubCategory);
                Dal.addSubCategory((SubCategory)newCategoryOrSubCategory);
            }
            else {
                tryAddCategoryInList(newCategoryOrSubCategory);
                Dal.addCategory(newCategoryOrSubCategory);
            }
        }

        private void tryRemoveSubCategoryInList(SubCategory subCategory) {
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

        private void tryAddSubCategoryInList(SubCategory subCategory) {
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

        private void tryRemoveCategoryInList(Category category) {
            if (category.VisibleInExpenses)
                ExpenseCategories.Remove(ExpenseCategories.FirstOrDefault(i => i.Category.Id == category.Id));

            if (category.VisibleInIncomes)
                IncomeCategories.Remove(ExpenseCategories.FirstOrDefault(i => i.Category.Id == category.Id));
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

        private void tryAddCategoryInList(Category category) {
            if (category.VisibleInExpenses)
                ExpenseCategories.Insert(0, new CategoryWithSubCategories {
                    Category = category
                });

            if (category.VisibleInIncomes)
                IncomeCategories.Insert(0, new CategoryWithSubCategories {
                    Category = category
                });
        }

        private bool isCategoryVisibleInCurrentView(Category category) {
            return ((bool)ExpenseRadioButton.IsChecked && category.VisibleInExpenses) ||
                   ((bool)IncomeRadioButton.IsChecked && category.VisibleInIncomes);
        }


        private async void RadioButton_Checked(object sender, RoutedEventArgs e) {
            await Task.Delay(5);
            Categories = ExpenseCategories;
        }

        private async void RadioButton_Checked_1(object sender, RoutedEventArgs e) {
            await Task.Delay(5);
            Categories = IncomeCategories;
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
