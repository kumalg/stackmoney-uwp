using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Microsoft.Graphics.Canvas.Effects;
using Finanse.Models.SystemVersion;

namespace Finanse {
    public class BackDropInside : Canvas {

#if SDKVERSION_15063
        private SpriteVisual _hostVisual;
        private Compositor _compositor;
#endif

        public BackDropInside() {
            if (SystemCurrentVersion.Revision < SystemVersions.AnniversaryUpdate)
                return;

#if SDKVERSION_15063
            Initialize();
            this.SizeChanged += FrostHostOnSizeChanged;
#endif

        }

#if SDKVERSION_15063
        private void FrostHostOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            if (_hostVisual != null)
                _hostVisual.Size = new System.Numerics.Vector2((float)ActualWidth, (float)ActualHeight);
        }

        private void Initialize() {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            var frostEffect = new GaussianBlurEffect {
                BlurAmount = 16.0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new CompositionEffectSourceParameter("backdropBrush")
            };

            // Create an instance of the effect and set its source to a CompositionBackdropBrush
            var effectFactory = _compositor.CreateEffectFactory(frostEffect);
            var backdropBrush = _compositor.CreateBackdropBrush();
            var effectBrush = effectFactory.CreateBrush();

            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

            // Create a Visual to contain the frosted glass effect
            _hostVisual = _compositor.CreateSpriteVisual();
            _hostVisual.Brush = effectBrush;
            _hostVisual.Size = new System.Numerics.Vector2((float)ActualWidth,(float)ActualHeight);

            // Add the blur as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(this, _hostVisual);
        }
#endif
    }
}
