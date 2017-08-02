using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models.Categories;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Finanse.Models.Helpers;

namespace Finanse.Pages {
    //TODO można zapychać bezsensownie sql przy zamianach z kategorii na podkategorie i w drugą stronę. 
    //TODO Trzeba sprawdzać, że gdy już w bazie istnieje element z takim GlobalId to tylko ma zaaktualizować
    public sealed partial class CategoriesPage : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<CategoryWithSubCategories> _expenseCategories;
        private ObservableCollection<CategoryWithSubCategories> ExpenseCategories {
            get => _expenseCategories;
            set {
                _expenseCategories = value;
                RaisePropertyChanged(nameof(ExpenseCategories));
            }
        }

        private ObservableCollection<CategoryWithSubCategories> _incomeCategories;
        private ObservableCollection<CategoryWithSubCategories> IncomeCategories {
            get => _incomeCategories;
            set {
                _incomeCategories = value;
                RaisePropertyChanged(nameof(IncomeCategories));
            }
        }

        public CategoriesPage() {
            InitializeComponent();
            Loaded += CategoriesPage_Loaded;
        }

        private async void CategoriesPage_Loaded(object sender, RoutedEventArgs e) {
            if (ExpenseCategories == null)
                ExpensesProgressRingActivity(true);

            if (IncomeCategories == null)
                IncomesProgressRingActivity(true);

            var expenses = await CategoriesDal.GetCategoriesWithSubCategoriesInExpenses();
            ExpenseCategories = new ObservableCollection<CategoryWithSubCategories>(expenses);
            ExpensesProgressRingActivity(false);

            var incomes = await CategoriesDal.GetCategoriesWithSubCategoriesInIncomes();
            IncomeCategories = new ObservableCollection<CategoryWithSubCategories>(incomes);
            IncomesProgressRingActivity(false);
        }

        private void ExpensesProgressRingActivity(bool active) {
            ExpensesProgressRing.IsActive = active;
            ExpensesPivotProgressRing.IsActive = active;
        }

        private void IncomesProgressRingActivity(bool active) {
            IncomesProgressRing.IsActive = active;
            IncomesPivotProgressRing.IsActive = active;
        }

        private async void EditCategory_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            CategoryWithSubCategories thisCategorys = (CategoryWithSubCategories)datacontext;

            if (thisCategorys == null)
                return;

            if (thisCategorys.Category.CantDelete) {
                ShowCantEditDialog();
                return;
            }

            var contentDialogItem = new NewCategoryContentDialog(thisCategorys.Category);
            var result = await contentDialogItem.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            Category cat = contentDialogItem.NewCategoryItem;

            if (cat is SubCategory)
                UpdateCategory(thisCategorys, cat as SubCategory);
            else
                UpdateCategory(thisCategorys, cat);
        }

        private void UpdateCategory(CategoryWithSubCategories categoryWithSubCategories, Category category) {

            CategoryWithSubCategories newCategoryWithSubCategorie = new CategoryWithSubCategories {
                Category = category,
                SubCategories = categoryWithSubCategories.SubCategories
            };

            int indexInExpenses = -1, indexInIncomes = -1;

            if (categoryWithSubCategories.Category.VisibleInExpenses)
                indexInExpenses = ExpenseCategories.IndexOf(categoryWithSubCategories);
            if (categoryWithSubCategories.Category.VisibleInIncomes)
                indexInIncomes = IncomeCategories.IndexOf(categoryWithSubCategories);

            TryRemoveCategoryWithSubCategoriesInList(categoryWithSubCategories);
            TryInsertCategoryWithSubCategoriesInList(indexInExpenses, indexInIncomes, newCategoryWithSubCategorie);

            CategoriesDal.UpdateCategory(category);
        }

        private void UpdateCategory(CategoryWithSubCategories categoryWithSubCategories, SubCategory subCategory) {
            TryRemoveCategoryWithSubCategoriesInList(categoryWithSubCategories);
            TryAddSubCategoryInList(subCategory);

            CategoriesDal.RemoveCategoryWithSubCategories(categoryWithSubCategories.Category);
            CategoriesDal.AddCategory(subCategory);
        }

        private async void EditSubCategory_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            SubCategory thisSubCategory = (SubCategory)datacontext;

            if (thisSubCategory != null && thisSubCategory.CantDelete) {
                ShowCantEditDialog();
                return;
            }

            var contentDialogItem = new NewCategoryContentDialog(thisSubCategory);
            var result = await contentDialogItem.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            Category cat = contentDialogItem.NewCategoryItem;

            if (cat is SubCategory)
                UpdateSubCategory(thisSubCategory, cat as SubCategory);
            else
                UpdateSubCategory(thisSubCategory, cat);
        }

        private void UpdateSubCategory(SubCategory oldSubCategory, Category category) {
            TryRemoveSubCategoryInList(oldSubCategory);
            TryAddCategoryInList(category);

            CategoriesDal.RemoveSubCategoryAndSetOperationsToParentCategory(oldSubCategory);
            CategoriesDal.AddCategory(category);
        }

        private void UpdateSubCategory(SubCategory oldSubCategory, SubCategory subCategory) {
            TryRemoveSubCategoryInList(oldSubCategory);
            TryAddSubCategoryInList(subCategory);
            CategoriesDal.UpdateCategory(subCategory);
        }

        private async void DeleteCategory_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            Category category = ((CategoryWithSubCategories)datacontext)?.Category;

            if (category == null)
                return;

            if (category.CantDelete) {
                ShowCantDeleteDialog();
                return;
            }

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć kategorię i wszystkie jej podkategorie?");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;
            
            TryRemoveCategoryInList(category);
            CategoriesDal.RemoveCategoryWithSubCategories(category);
        }

        private async void ShowCantDeleteDialog() {
            MessageDialog messageDialog = new MessageDialog("Nie można usunąć tej kategorii");
            await messageDialog.ShowAsync();
        }
        private async void ShowCantEditDialog() {
            MessageDialog messageDialog = new MessageDialog("Nie można edytować tej kategorii");
            await messageDialog.ShowAsync();
        }

        private async void DeleteSubCategory_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            SubCategory subCategory = (SubCategory)datacontext;

            if (subCategory == null)
                return;

            if (subCategory.CantDelete) {
                ShowCantDeleteDialog();
                return;
            }

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć podkategorię?");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            TryRemoveSubCategoryInList(subCategory);
            CategoriesDal.RemoveSubCategoryAndSetOperationsToParentCategory(subCategory);
        }

        private async void AddSubCat_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            CategoryWithSubCategories categoryWithSubCategories = (CategoryWithSubCategories)datacontext;

            if (categoryWithSubCategories == null)
                return;

            NewCategoryContentDialog contentDialogItem = new NewCategoryContentDialog(categoryWithSubCategories.Category.GlobalId);
            ContentDialogResult result = await contentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary)
                AddNewCategoryOrSubCategoryToListAndSql(contentDialogItem.NewCategoryItem);
        }

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            NewCategoryContentDialog contentDialogItem = new NewCategoryContentDialog();
            ContentDialogResult result = await contentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary)
                AddNewCategoryOrSubCategoryToListAndSql(contentDialogItem.NewCategoryItem);
        }

        private void AddNewCategoryOrSubCategoryToListAndSql(Category newCategoryOrSubCategory) {
            CategoriesDal.AddCategory(newCategoryOrSubCategory);

            if (newCategoryOrSubCategory is SubCategory)
                TryAddSubCategoryInList((SubCategory)newCategoryOrSubCategory);
            else
                TryAddCategoryInList(newCategoryOrSubCategory);
        }

        private void TryRemoveSubCategoryInList(SubCategory subCategory) {
            if (subCategory.VisibleInExpenses)
                ExpenseCategories
                    .FirstOrDefault(item => item.Category.GlobalId == subCategory.BossCategoryId)
                    .SubCategories
                    .Remove(subCategory);

            if (subCategory.VisibleInIncomes)
                IncomeCategories
                    .FirstOrDefault(item => item.Category.GlobalId == subCategory.BossCategoryId)
                    .SubCategories
                    .Remove(subCategory);
        }

        private void TryAddSubCategoryInList(SubCategory subCategory) {
            if (subCategory.VisibleInExpenses)
                ExpenseCategories
                    .FirstOrDefault(item => item.Category.GlobalId == subCategory.BossCategoryId)?
                    .SubCategories.Insert(0, subCategory);

            if (subCategory.VisibleInIncomes)
                IncomeCategories
                    .FirstOrDefault(item => item.Category.GlobalId == subCategory.BossCategoryId)?
                    .SubCategories.Insert(0, subCategory);
        }

        private void TryRemoveCategoryWithSubCategoriesInList(CategoryWithSubCategories categoryWithSubCategories) {
            if (categoryWithSubCategories.Category.VisibleInExpenses)
                ExpenseCategories.Remove(categoryWithSubCategories);

            if (categoryWithSubCategories.Category.VisibleInIncomes)
                IncomeCategories.Remove(categoryWithSubCategories);
        }

        private void TryRemoveCategoryInList(Category category) {
            var expenseCategories =  ExpenseCategories;

            if (category.VisibleInExpenses)
                expenseCategories.Remove(expenseCategories.FirstOrDefault(i => i.Category.Id == category.Id));

            var incomeCategories = IncomeCategories;

            if (category.VisibleInIncomes)
                incomeCategories.Remove(incomeCategories.FirstOrDefault(i => i.Category.Id == category.Id));
        }

        private void TryInsertCategoryWithSubCategoriesInList(int indexInExpenses, int indexInIncomes, CategoryWithSubCategories categoryWithSubCategories) {
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

        private void TryAddCategoryInList(Category category) {
            if (category.VisibleInExpenses)
                ExpenseCategories.Insert(0, new CategoryWithSubCategories {
                    Category = category
                });

            if (category.VisibleInIncomes)
                IncomeCategories.Insert(0, new CategoryWithSubCategories {
                    Category = category
                });
        }

        private void Element_RightTapped(object sender, RightTappedRoutedEventArgs e) => Flyouts.ShowFlyoutBase(sender);
        private void Element_RightTapped(object sender, TappedRoutedEventArgs e) => Flyouts.ShowFlyoutBase(sender);
    }
}
