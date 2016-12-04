﻿using System;
using System.Collections.Generic;
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
using Finanse.Elements;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Globalization;
using Finanse.DataAccessLayer;
using Finanse.Models;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Dialogs {

    public sealed partial class OperationDetailsContentDialog : ContentDialog {

        private readonly ObservableCollection<GroupInfoList<Operation>> _source;
        Operation editedOperation;
        string whichOption;

        public OperationDetailsContentDialog(ObservableCollection<GroupInfoList<Operation>> _source, Operation editedOperation, string whichOption) {

            this.InitializeComponent();
            this._source = _source;
            this.editedOperation = editedOperation;
            this.whichOption = whichOption;

            if (whichOption == "pattern")
                Title = "Szczegóły szablonu";

            if (editedOperation.isExpense) {
                CostValue.Foreground = (SolidColorBrush)Application.Current.Resources["RedColorStyle"] as SolidColorBrush;
                CostValueIcon.Glyph = "";
            }
            else {
                CostValue.Foreground = (SolidColorBrush)Application.Current.Resources["GreenColorStyle"] as SolidColorBrush;
                CostValueIcon.Glyph = "";
            }

            NameValue.Visibility = Visibility.Collapsed;
            if (!String.IsNullOrEmpty(editedOperation.Title)) {
                NameValue.Text = editedOperation.Title;
                NameValue.Visibility = Visibility.Visible;
            }

            CostValue.Text = editedOperation.Cost.ToString("C", Settings.GetActualCurrency());

            DateValuePanel.Visibility = Visibility.Collapsed;
            if (!editedOperation.Date.Equals("") && whichOption != "pattern") {
                DateValue.Text = String.Format("{0:dddd, dd MMMM yyyy}", Convert.ToDateTime(editedOperation.Date));
                DateValuePanel.Visibility = Visibility.Visible;
            }

            OperationCategory cat = Dal.GetOperationCategoryById(editedOperation.CategoryId);
            OperationSubCategory subCat = Dal.GetOperationSubCategoryById(editedOperation.SubCategoryId);
            MoneyAccount account = Dal.GetMoneyAccountById(editedOperation.MoneyAccountId);

            /* KATEGORIA */
            CategoryValuePanel.Visibility = Visibility.Collapsed;
            if (cat != null) {
                CategoryValue.Text = cat.Name;
                CategoryValuePanel.Visibility = Visibility.Visible;
            }

            /* PODKATEGORIA */
            SubCategoryValuePanel.Visibility = Visibility.Collapsed;
            if (subCat != null) {
                SubCategoryValue.Text = subCat.Name;
                SubCategoryValuePanel.Visibility = Visibility.Visible;
            }

            /* FORMA PŁATNOŚCI */
            PayFormPanel.Visibility = Visibility.Collapsed;
            if (account != null) {
                PayForm.Text = account.Name;
                PayFormPanel.Visibility = Visibility.Visible;
            }

            /* WIĘCEJ INFORMACJI */
            if (!String.IsNullOrEmpty(editedOperation.MoreInfo)) {
                MoreInfo.Text = editedOperation.MoreInfo;
            }
            else
                MoreInfoPanel.Visibility = Visibility.Collapsed;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {

            Hide();

            var ContentDialogItem = new Delete_ContentDialog(_source, editedOperation, whichOption);

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            Hide();

            //string whichOptionLocal;

            NewOperationContentDialog ContentDialogItem;

            switch (whichOption) {
                case "pattern": {
                        ContentDialogItem = new NewOperationContentDialog(null, editedOperation, true);
                        break;
                    }
                default: {
                        ContentDialogItem = new NewOperationContentDialog(_source, editedOperation);
                        break;
                    }
            }

            var result = await ContentDialogItem.ShowAsync();
        }
    }
}
