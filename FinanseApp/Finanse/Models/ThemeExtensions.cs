using Windows.UI.Xaml;

namespace Finanse.Models
{
    public static class ThemeExtensions
    {
        public static ApplicationTheme ToApplicationTheme(this ElementTheme theme)
        {
            return theme == ElementTheme.Dark
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;
        }
    }
}
