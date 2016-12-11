using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Slideshow
{
    class Gallery
    {
        private readonly CoreDispatcher dispatcher;
        private readonly Image image;
        private readonly IList<BitmapImage> photos;
        private int index = 0;

        public Gallery(CoreDispatcher dispatcher, IEnumerable<StorageFile> photos, Image image)
        {
            this.dispatcher = dispatcher;
            this.image = image;
            this.photos = photos.Select(async photo => await LoadImage(photo)).Select(task => task.Result).ToList();
        }

        public void Start()
        {
            var periodicTimer =
                ThreadPoolTimer.CreatePeriodicTimer(
                    async (source) =>
                    {
                        await
                            this.dispatcher.RunAsync(CoreDispatcherPriority.High,
                                () => { this.image.Source = this.photos[this.index++]; });
                    }, TimeSpan.FromSeconds(10));
        }

        private static async Task<BitmapImage> LoadImage(StorageFile file)
        {
            var bitmapImage = new BitmapImage();
            var stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

            bitmapImage.SetSource(stream);

            return bitmapImage;
        }
    }
}
