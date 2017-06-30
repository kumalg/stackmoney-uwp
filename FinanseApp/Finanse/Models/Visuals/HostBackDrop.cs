using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Finanse.Models.SystemVersion;
using Microsoft.Graphics.Canvas.Effects;

namespace Finanse.Models.Visuals {
    public class HostBackDrop : Control {

#if SDKVERSION_15063
        private Compositor _compositor;
        private GaussianBlurEffect _frostEffect;
        private CompositionEffectFactory _effectFactory;
        private CompositionBackdropBrush _backdropBrush;
        private CompositionEffectBrush _effectBrush;

        private SpriteVisual _frostVisual;
#endif

        public HostBackDrop() {
            if (SystemCurrentVersion.Revision < SystemVersions.CreatorsUpdate)
                return;

#if SDKVERSION_15063
            Initialize();
            this.SizeChanged += FrostHostOnSizeChanged;
#endif

        }

#if SDKVERSION_15063
        private void FrostHostOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            if (_frostVisual != null)
                _frostVisual.Size = new Vector2((float)ActualWidth, (float)ActualHeight);
        }

        private void Initialize() {

            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            _frostEffect = new GaussianBlurEffect {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("backdropBrush")
            };

            // Create an instance of the effect and set its source to a CompositionBackdropBrush
            _effectFactory = _compositor.CreateEffectFactory(_frostEffect);
            _backdropBrush = _compositor.CreateHostBackdropBrush();
            _effectBrush = _effectFactory.CreateBrush();

            // Create a Visual to contain the frosted glass effect
            _frostVisual = _compositor.CreateSpriteVisual();
            _frostVisual.Brush = _effectBrush;
            _frostVisual.Size = new Vector2((float)ActualWidth, (float)ActualHeight);

            _effectBrush.SetSourceParameter("backdropBrush", _backdropBrush);

            // Add the blur as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(this, _frostVisual);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(HostBackDrop), new PropertyMetadata(Colors.Blue));

        public Color Color {
            get {
                return (Color)GetValue(ColorProperty);
            }
            set {
                SetValue(ColorProperty, value);
                _frostEffect = new GaussianBlurEffect {
                    Name = "Blur",
                    BlurAmount = 0.0f,
                    BorderMode = EffectBorderMode.Hard,
                    Optimization = EffectOptimization.Balanced,
                    Source = new CompositionEffectSourceParameter("backdropBrush")
                };

                // Create an instance of the effect and set its source to a CompositionBackdropBrush
                _effectFactory = _compositor.CreateEffectFactory(_frostEffect);
                _backdropBrush = _compositor.CreateHostBackdropBrush();
                _effectBrush = _effectFactory.CreateBrush();

                // Create a Visual to contain the frosted glass effect
                _frostVisual = _compositor.CreateSpriteVisual();
                _frostVisual.Brush = _effectBrush;
                _frostVisual.Size = new Vector2((float)ActualWidth, (float)ActualHeight);

                _effectBrush.SetSourceParameter("backdropBrush", _backdropBrush);

                // Add the blur as a child of the host in the visual tree
                ElementCompositionPreview.SetElementChildVisual(this, _frostVisual);
            }
        }
#endif
    }
}
