using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {

    public sealed partial class OperationPatternsContentDialog : ContentDialog {

        private readonly List<OperationPattern> OperationPatterns = Dal.GetAllPatterns();

        public OperationPatternsContentDialog() {
            InitializeComponent();
        }

        private void OperationPatternsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Hide();
        }

        public Operation SetOperation () {
            return OperationPatternsListView.SelectedIndex == -1 ?
                null 
                : ((OperationPattern)OperationPatternsListView.SelectedItem).ToOperation();
        }
    }
}
