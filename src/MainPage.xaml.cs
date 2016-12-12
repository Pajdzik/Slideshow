using System;
using System.IO;
using System.Numerics;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
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
            this.InitializeGallery();

            //this.Page_Loaded();
        }

        private void InitializeGallery()
        {
            var library = new PhotoLibrary();
            var gallery = new Gallery(this.Dispatcher, library, this.CurrentImage, this.ProgressRing);
            gallery.Start();
        }

        private async void F()
        {
            var file = await Package.Current.InstalledLocation.GetFileAsync("test.png");
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                var device = new CanvasDevice();
                var bitmap = await CanvasBitmap.LoadAsync(device, stream);
                var renderer = new CanvasRenderTarget(device, bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height, bitmap.Dpi);

                using (var ds = renderer.CreateDrawingSession())
                {
                    var blur = new GaussianBlurEffect();
                    blur.BlurAmount = 8.0f;
                    blur.BorderMode = EffectBorderMode.Hard;
                    blur.Optimization = EffectOptimization.Quality;
                    blur.Source = bitmap;
                    ds.DrawImage(blur);
                }
            }
        }

    }
}