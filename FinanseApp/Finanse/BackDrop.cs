using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;

namespace Finanse {
    public class BackDrop : Control {

        private Compositor compositor;
        private GaussianBlurEffect frostEffect;
        private CompositionEffectFactory effectFactory;
        private CompositionBackdropBrush backdropBrush;
        private CompositionEffectBrush effectBrush;

        private SpriteVisual frostVisual;

        public BackDrop() {
            Initialize();
            this.SizeChanged += FrostHostOnSizeChanged;
        }

        private void FrostHostOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            if (frostVisual != null)
                frostVisual.Size = new System.Numerics.Vector2((float)ActualWidth, (float)ActualHeight);
        }

        private void Initialize() {

            compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            frostEffect = new GaussianBlurEffect() {
                Name = "Blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("backdropBrush")
            };

            // Create an instance of the effect and set its source to a CompositionBackdropBrush
            effectFactory = compositor.CreateEffectFactory(frostEffect);
            backdropBrush = compositor.CreateHostBackdropBrush();
            effectBrush = effectFactory.CreateBrush();

            // Create a Visual to contain the frosted glass effect
            frostVisual = compositor.CreateSpriteVisual();
            frostVisual.Brush = effectBrush;
            frostVisual.Size = new System.Numerics.Vector2((float)ActualWidth, (float)ActualHeight);

            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

            // Add the blur as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(this, frostVisual);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(BackDrop), new PropertyMetadata(Colors.Blue));

        public Color Color {
            get {
                return (Color)GetValue(ColorProperty);
            }
            set {
                SetValue(ColorProperty, value);
                frostEffect = new GaussianBlurEffect() {
                    Name = "Blur",
                    BlurAmount = 0.0f,
                    BorderMode = EffectBorderMode.Hard,
                    Optimization = EffectOptimization.Balanced,
                    Source = new CompositionEffectSourceParameter("backdropBrush")
                };

                // Create an instance of the effect and set its source to a CompositionBackdropBrush
                effectFactory = compositor.CreateEffectFactory(frostEffect);
                backdropBrush = compositor.CreateHostBackdropBrush();
                effectBrush = effectFactory.CreateBrush();

                // Create a Visual to contain the frosted glass effect
                frostVisual = compositor.CreateSpriteVisual();
                frostVisual.Brush = effectBrush;
                frostVisual.Size = new System.Numerics.Vector2((float)ActualWidth, (float)ActualHeight);

                effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

                // Add the blur as a child of the host in the visual tree
                ElementCompositionPreview.SetElementChildVisual(this, frostVisual);
            }
        }


    }
}
