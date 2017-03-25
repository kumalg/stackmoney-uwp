using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {
    public sealed partial class AcceptContentDialog : ContentDialog {

        public AcceptContentDialog(string message) {
            InitializeComponent();
            Title = message;
        }
    }
}
