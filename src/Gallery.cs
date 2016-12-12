using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace Slideshow
{
    internal class Gallery
    {
        private readonly CoreDispatcher dispatcher;
        private readonly Image image;
        private readonly Grid mainGrid;
        private readonly IList<StorageFile> photos;
        private readonly IList<StorageFile> newPhotos;
        private readonly Random random = new Random();
        private readonly Compositor compositor;

        public Gallery(CoreDispatcher dispatcher, IEnumerable<StorageFile> photos, Image image, Grid mainGrid)
        {
            this.dispatcher = dispatcher;
            this.photos = photos.ToList();
            this.image = image;
            this.mainGrid = mainGrid;
            this.newPhotos = this.PickNewPhotos();
            //this.compositor = ElementCompositionPreview.GetElementVisual(mainGrid).Compositor;
        }

        public void Start()
        {
            var periodicTimer =
                ThreadPoolTimer.CreatePeriodicTimer(
                    async (source) =>
                    {
                        await this.dispatcher.RunAsync(CoreDispatcherPriority.High,
                            async () =>
                            {
                                var bitmap = await this.GetBitmapImage();
                                this.image.Source = bitmap;

                                await this.Foo();
                            });
                    }, TimeSpan.FromSeconds(0.5));
        }

        private async Task<BitmapImage> GetBitmapImage()
        {
            var photo = this.PickNextPhoto();
            var bitmap = await LoadImage(photo);

            return bitmap;
        }

        private static async Task<BitmapImage> LoadImage(StorageFile file)
        {
            var bitmapImage = new BitmapImage();
            var stream = (FileRandomAccessStream) await file.OpenAsync(FileAccessMode.Read);

            bitmapImage.SetSource(stream);

            return bitmapImage;
        }

        private StorageFile PickNextPhoto()
        {
            var pickFromNewPhotos = this.random.Next()%2 == 1;
            if (pickFromNewPhotos)
            {
                return this.GetRandomPhoto(this.newPhotos);
            }

            return this.GetRandomPhoto(this.photos);
        }

        private StorageFile GetRandomPhoto(IList<StorageFile> photos)
        {
            return photos[this.random.Next()%photos.Count];
        }

        private IList<StorageFile> PickNewPhotos()
        {
            return this.photos.OrderByDescending(photo => photo.DateCreated).Take(50).ToList();
        }
    }
}