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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Elements {
    public sealed partial class Delete_ContentDialog : ContentDialog {
        ObservableCollection<Operation> Operations;
        SQLite.Net.SQLiteConnection conn;
        Operation operation;
        public Delete_ContentDialog(ObservableCollection<Operation> Operations, SQLite.Net.SQLiteConnection conn, Operation operation) {
            this.InitializeComponent();
            this.Operations = Operations;
            this.conn = conn;
            this.operation = operation;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            Operations.Remove(operation);
            conn.Delete(operation);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        }
    }
}
