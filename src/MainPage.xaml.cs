using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.Effects;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Slideshow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CompositionEffectBrush _brush;
        private readonly Compositor _compositor;

        public MainPage()
        {
            this.InitializeComponent();
            this._compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            this.BlendSelection_SelectionChanged();
            this.InitializeGallery();
        }

        private void InitializeGallery()
        {
            var library = new PhotoLibrary();
            var gallery = new Gallery(this.Dispatcher, library, this.CurrentImage, this.BackgroundImage, this.ProgressRing);
            gallery.Start();
        }

        private void BlendSelection_SelectionChanged()
        {
            BlendEffectMode blendmode = BlendEffectMode.Multiply;

            // Create a chained effect graph using a BlendEffect, blending color and blur
            var graphicsEffect = new BlendEffect
            {
                Mode = blendmode,
                Background = new ColorSourceEffect()
                {
                    Name = "Tint",
                    Color = Color.FromArgb(0, 255, 0, 0),
                },

                Foreground = new GaussianBlurEffect()
                {
                    Name = "Blur",
                    Source = new CompositionEffectSourceParameter("Backdrop"),
                    BlurAmount = 100,
                    BorderMode = EffectBorderMode.Hard,
                }
            };

            var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect,
                new[] { "Blur.BlurAmount", "Tint.Color" });

            // Create EffectBrush, BackdropBrush and SpriteVisual
            _brush = blurEffectFactory.CreateBrush();

            var destinationBrush = _compositor.CreateBackdropBrush();
            _brush.SetSourceParameter("Backdrop", destinationBrush);
            _brush.Properties.InsertScalar("Blur.BlurAmount", 100);

            var blurSprite = _compositor.CreateSpriteVisual();
            blurSprite.Size = new Vector2((float)BackgroundImage.ActualWidth, (float)BackgroundImage.ActualHeight);
            blurSprite.Brush = _brush;

            ElementCompositionPreview.SetElementChildVisual(BackgroundImage, blurSprite);
        }

        private void BackgroundImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SpriteVisual blurVisual = (SpriteVisual)ElementCompositionPreview.GetElementChildVisual(BackgroundImage);

            if (blurVisual != null)
            {
                blurVisual.Size = e.NewSize.ToVector2();
            }

        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                this.FullScreenButton.Symbol = Symbol.FullScreen;
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
            else
            {
                this.FullScreenButton.Symbol = Symbol.BackToWindow;
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }
        }
    }
}