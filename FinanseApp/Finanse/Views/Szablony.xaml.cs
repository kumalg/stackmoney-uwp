using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Szablony : Page
    {
        public Szablony()
        {
            this.InitializeComponent();
        }

        private async void ButtonShowContentDialog1_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var dialog = new ContentDialog()
            {
                Title = "Dark Theme Content Dialog",
                RequestedTheme = ElementTheme.Dark,
                //FullSizeDesired = true,
                MaxWidth = this.ActualWidth // Required for Mobile!
            };

            // Setup Content
            var panel = new StackPanel();

            panel.Children.Add(new TextBlock
            {
                Text = "It is a long established fact that a reader will be distracted by the readable. " +
                                "Content of a page when looking at its layout.",
                TextWrapping = TextWrapping.Wrap,
            });

            var cb = new CheckBox
            {
                Content = "Agree"
            };

            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding
            {
                Source = dialog,
                Path = new PropertyPath("IsPrimaryButtonEnabled"),
                Mode = BindingMode.TwoWay,
            });

            panel.Children.Add(cb);
            dialog.Content = panel;

            // Add Buttons
            dialog.PrimaryButtonText = "OK";
            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonClick += delegate
            {
                btn.Content = "Result: OK";
            };

            dialog.SecondaryButtonText = "Cancel";
            dialog.SecondaryButtonClick += delegate
            {
                btn.Content = "Result: Cancel";
            };

            // Show Dialog
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                btn.Content = "Result: NONE";
            }
        }
    }
}
