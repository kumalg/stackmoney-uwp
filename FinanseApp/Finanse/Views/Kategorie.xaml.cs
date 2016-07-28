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

        string path;
        SQLite.Net.SQLiteConnection conn;

        public ObservableCollection<OperationCategory> OperationCategories = new ObservableCollection<OperationCategory>();
        public ObservableCollection<OperationCategory> OperationSubCategories = new ObservableCollection<OperationCategory>();

        public OperationCategory operationCategoryItem;
        public OperationSubCategory operationSubCategoryItem;

        public Kategorie() {

            this.InitializeComponent();

            path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db.sqlite");
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            conn.CreateTable<OperationCategory>();
            conn.CreateTable<OperationSubCategory>();

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

        private async void NewCategory_Click(object sender, RoutedEventArgs e) {
            var ContentDialogItem = new NewCategoryContentDialog(OperationCategories, OperationSubCategories, conn);

            var result = await ContentDialogItem.ShowAsync();
        }
    }
}
