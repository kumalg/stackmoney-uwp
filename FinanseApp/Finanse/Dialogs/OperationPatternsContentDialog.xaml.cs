﻿using Finanse.DataAccessLayer;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Finanse.Models.Operations;

namespace Finanse.Dialogs {

    public sealed partial class OperationPatternsContentDialog {

        private readonly List<OperationPattern> _operationPatterns = Dal.GetAllPatterns();

        public OperationPatternsContentDialog() {
            InitializeComponent();
        }

        private void OperationPatternsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => Hide();

        public Operation SetOperation () {
            return OperationPatternsListView.SelectedIndex == -1 ?
                null 
                : ((OperationPattern)OperationPatternsListView.SelectedItem).ToOperation();
        }
    }
}
