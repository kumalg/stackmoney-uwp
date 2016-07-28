using SQLite.Net.Attributes;

using System;
using System.Collections.Generic;
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
using Finanse.Elements;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class Strona_glowna : Page {

        string path;
        SQLite.Net.SQLiteConnection conn;

        public ObservableCollection<Operation> Operations = new ObservableCollection<Operation>();
        public List<OperationCategory> OperationCategories = new List<OperationCategory>();

        public OperationCategory operationCategoryItem;
        public OperationSubCategory operationSubCategoryItem;

        public Strona_glowna() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
            conn.CreateTable<Operation>();
            conn.CreateTable<OperationCategory>();
            conn.CreateTable<OperationSubCategory>();

            var queryOperation = conn.Table<Operation>();

            foreach (var message in queryOperation) {
                Operations.Add(new Operation {
                    Title = message.Title,
                    Cost = message.Cost,
                    Category = message.Category,
                    ExpenseOrIncome = message.ExpenseOrIncome
                });
            }

            var queryOperationCategory = conn.Table<OperationCategory>();
            var queryOperationSubCategory = conn.Table<OperationSubCategory>();

            foreach (var message in queryOperationCategory) {
                operationCategoryItem = new OperationCategory {
                    Name = message.Name,
                    Color = message.Color,
                    Icon = message.Icon,
                };

                foreach (var submessage in queryOperationSubCategory) {
                    if (submessage.BossCategory == message.Name) {
                        operationSubCategoryItem = new OperationSubCategory {
                            Name = submessage.Name,
                            Color = submessage.Color,
                            Icon = submessage.Icon,
                        };
                        operationCategoryItem.addSubCategory(operationSubCategoryItem);
                    }
                }
                OperationCategories.Add(operationCategoryItem);
            }
        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e) {

            var ContentDialogItem = new NewOperationContentDialog(Operations, OperationCategories, conn);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
             FrameworkElement senderElement = sender as FrameworkElement;
             FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
             flyoutBase.ShowAt(senderElement);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            //this datacontext is probably some object of some type T
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            //this datacontext is probably some object of some type T
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }
    }
}
