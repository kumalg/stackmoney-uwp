using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Models {
    public class TextBoxEvents {
        public void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            var textBox = sender as TextBox;
            if (textBox != null)
                textBox.Text = textBox.Text.Trim();
        }
    }
}
