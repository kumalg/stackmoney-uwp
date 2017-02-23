using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using System;

namespace Finanse.Dialogs {
    public sealed partial class AcceptContentDialog : ContentDialog {

        public AcceptContentDialog(string message) {
            InitializeComponent();
            Title = message;
        }
    }
}
