﻿using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Finanse.Models.Helpers;
using Finanse.Models.WhatsNew;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace Finanse.Pages {

    public sealed partial class SettingsPage {

        public string AppVersion = Informations.AppVersion;
        public string DisplayName = Informations.DisplayName;
        public string PublisherDisplayName = Informations.PublisherDisplayName;

        public SettingsPage() {

            InitializeComponent();

            foreach (CultureInfo item in Settings.AllCurrencies) {
                CurrencyValue.Items?.Add(new ComboBoxItem {
                    Content = new RegionInfo(item.Name).ISOCurrencySymbol,
                    Tag = item.Name
                });
            }

            if (Settings.Theme== ApplicationTheme.Dark)
                DarkThemeRadioButton.IsChecked = true;
            else
                LightThemeRadioButton.IsChecked = true;

            DarkThemeRadioButton.Checked += DarkThemeRadioButton_Checked;
            LightThemeRadioButton.Checked += LightThemeRadioButton_Checked;

            CurrencyValue.SelectedItem = CurrencyValue
                .Items
                .OfType<ComboBoxItem>()
                .SingleOrDefault(i => i.Tag.Equals(Settings.ActualCultureInfo.Name));

            for (int i = 1; i <= 12; i++)
                MaxNumberOfNextMonth.Items?.Add(new ComboBoxItem {
                    Content = i,
                });

            MaxNumberOfNextMonth.SelectedIndex = Settings.MaxFutureMonths- 1;
            CategoryNameVisibilityToggleButton.IsOn = Settings.CategoryNameVisibility;
            AccountEllipseVisibilityToggleButton.IsOn = Settings.AccountEllipseVisibility;
           
            SyncSettingsToggleButton.IsOn = Settings.SyncSettings;
            SyncSettingsToggleButton.Toggled += SyncSettingsToggleButton_Toggled;

            SyncDataToggleButton.IsOn = Settings.SyncData;
            SyncDataToggleButton.Toggled += SyncDataToggleButton_Toggled;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var cultureString = (string)( (ComboBoxItem)CurrencyValue.SelectedItem )?.Tag;
            Settings.ActualCultureInfo = new CultureInfo(cultureString);
        }

        private void MaxNumberOfFutureMonths_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (ComboBoxItem)( (ComboBox)sender ).SelectedItem;
            if (item?.Content != null)
                Settings.MaxFutureMonths = (int)item.Content;
        }

        private void CategoryNameVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.CategoryNameVisibility = CategoryNameVisibilityToggleButton.IsOn;
        }

        private void AccountEllipseVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.AccountEllipseVisibility = AccountEllipseVisibilityToggleButton.IsOn;
        }

        private void ReminderToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (ReminderStackPanel != null)
                ReminderStackPanel.Visibility = ((ToggleSwitch)sender).IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SyncSettingsToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SyncSettings = SyncSettingsToggleButton.IsOn;
        }

        private void SyncDataToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SyncData = SyncDataToggleButton.IsOn;
        }

        private void DarkThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.Theme = ApplicationTheme.Dark;

            if (Frame != null)
                ( (ThemeAwareFrame)Frame ).AppTheme = ElementTheme.Dark;
        }

        private void LightThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.Theme = ApplicationTheme.Light;

            if (Frame != null)
                ( (ThemeAwareFrame)Frame ).AppTheme = ElementTheme.Light;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            ShowDeleteAllContentDialog();
        }

        private static async void ShowDeleteAllContentDialog() {

            AcceptContentDialog acceptContentDialog = new AcceptContentDialog("Jesteś pewien?");

            ContentDialogResult result = await acceptContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            Dal.DeleteAll();
            Dal.AddInitialElements();
        }

        private void MarkdownText_MarkdownRendered(object sender, MarkdownRenderedEventArgs e) { }

        private async void MarkdownTextBlock_Loading(FrameworkElement sender, object args) {
            if (sender is MarkdownTextBlock markdownTextBlock)
                markdownTextBlock.Text = await WhatsNewHelper.GetJsonStringAsync();
        }
    }
}
