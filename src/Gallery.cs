using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Slideshow
{
    internal class Gallery
    {
        private readonly CoreDispatcher dispatcher;
        private readonly Image image;
        private readonly IList<StorageFile> photos;
        private readonly IDictionary<StorageFile, BitmapImage> fileImages;
        private int index = 0;

        public Gallery(CoreDispatcher dispatcher, IEnumerable<StorageFile> photos, Image image)
        {
            this.dispatcher = dispatcher;
            this.photos = photos.ToList();
            this.image = image;
            this.fileImages = new Dictionary<StorageFile, BitmapImage>();
        }

        public async Task Start()
        {
            var periodicTimer =
                ThreadPoolTimer.CreatePeriodicTimer(
                    async (source) =>
                    {
                        await this.dispatcher.RunAsync(CoreDispatcherPriority.High,
                            async () =>
                            {
                                var photo = this.photos[this.index++];
                                if (!this.fileImages.Keys.Contains(photo))
                                {
                                    this.fileImages[photo] = await LoadImage(photo);
                                }

                                this.image.Source = this.fileImages[photo];
                            });
                    }, TimeSpan.FromSeconds(3));
        }

        private static async Task<BitmapImage> LoadImage(StorageFile file)
        {
            var bitmapImage = new BitmapImage();
            var stream = (FileRandomAccessStream) await file.OpenAsync(FileAccessMode.Read);

            bitmapImage.SetSource(stream);

            return bitmapImage;
        }
    }
}