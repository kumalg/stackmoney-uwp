using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Finanse.Models.Helpers {
    public class Flyouts {
        public static void ShowFlyoutBase(object sender) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }
    }
}
