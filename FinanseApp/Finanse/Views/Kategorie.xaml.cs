using Finanse.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class Kategorie : Page {

        public ObservableCollection<OperationCategory> OperationCategories = new ObservableCollection<OperationCategory>();
        public ObservableCollection<OperationCategory> OperationSubCategories = new ObservableCollection<OperationCategory>();
        private int i = 0;
        private int BossCategoryIndex = -1;

        public Kategorie() {

            OperationCategories.Add(new OperationCategory {
                Name = "Transport",
                Color = "#FF0b63c7",
                Icon = "\uE806",
                IsSubcategory = false
            });

            OperationSubCategories.Add(new OperationCategory {
                Name = "Samochód",
                Color = "#FF0b63c7",
                Icon = "\uEA5E",
                IsSubcategory = true,
                BossCategory = "Transport"
            });

            OperationSubCategories.Add(new OperationCategory {
                Name = "Taxi",
                Color = "#FF23be32",
                Icon = "\uEC81",
                IsSubcategory = true,
                BossCategory = "Transport"
            });

            OperationSubCategories.Add(new OperationCategory {
                Name = "Autobus",
                Color = "#FFe78c33",
                Icon = "\uE806",
                IsSubcategory = true,
                BossCategory = "Transport"
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Jedzenie",
                Color = "#FF5bc70b",
                Icon = "",
                IsSubcategory = false
            });

            OperationSubCategories.Add(new OperationCategory {
                Name = "McDonald's",
                Color = "#FFd9cd06",
                Icon = "\uEB50",
                IsSubcategory = true,
                BossCategory = "Jedzenie"
            });

            OperationSubCategories.Add(new OperationCategory {
                Name = "Pasibus",
                Color = "#FF3fd826",
                Icon = "\uE700",
                IsSubcategory = true,
                BossCategory = "Jedzenie"
            });

            OperationSubCategories.Add(new OperationCategory {
                Name = "Biedronka",
                Color = "#FFea2626",
                Icon = "\uE1D2",
                IsSubcategory = true,
                BossCategory = "Jedzenie"
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Alkohol",
                Color = "#FF138b99",
                Icon = "\uE94C",
                IsSubcategory = false
            });

            this.InitializeComponent();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e) {

        }

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, OperationSubCategories);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {

        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {

        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {

        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        public void SubOperacjeListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {

        }
        
        private void SubOperacjeListView_Loading(FrameworkElement sender, object args) {
            if (i >= OperationCategories.Count)
                i = 0;

            ListView SubOperacjeListView = sender as ListView;
            List<OperationCategory> elements = new List<OperationCategory>();
            ObservableCollection<OperationCategory> bossCategories = (ObservableCollection<OperationCategory>)OperacjeListView.ItemsSource;

            foreach (OperationCategory element in OperationSubCategories) {
                if (element.BossCategory == bossCategories[BossCategoryIndex].Name)
                    elements.Add(element);
            }
            if (elements.Count != 0)
                SubOperacjeListView.ItemsSource = elements;
            i++;
        }
        
        private void OperacjeListView_Loading(FrameworkElement sender, object args) {
            BossCategoryIndex++;
        }

        private void OperationCategoryTemplate_Loading(FrameworkElement sender, object args) {
            BossCategoryIndex++;
        }
    }
}
