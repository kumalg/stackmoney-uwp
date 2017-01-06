using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {
    public sealed partial class AcceptContentDialog : ContentDialog {

        bool _isAccepted = false;

        public AcceptContentDialog(string message) {

            this.InitializeComponent();
            this.Title = message;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            _isAccepted = true;
        }

        public bool isAccepted() {
            return _isAccepted;
        }
    }
}
