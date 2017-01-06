using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using PropertyChanged;

namespace Slideshow.Core
{
    [ImplementPropertyChanged]
    public class GalleryViewModel
    {
        private readonly CoreDispatcher dispatcher;
        private readonly PhotoLibrary photoLibrary;
        private readonly Random random = new Random();

        public GalleryViewModel(CoreDispatcher dispatcher)
        {
            this.photoLibrary = new PhotoLibrary();
            this.dispatcher = dispatcher;
        }

        public ImageSource ImageSource { get; set; }

        public bool IsLoading { get; set; }

        public async Task Start()
        {
            this.IsLoading = true;
            await this.LoadImages();
            this.IsLoading = false;

            await this.ShowPhoto();

            ThreadPoolTimer.CreatePeriodicTimer(
                async (source) =>
                {
                    await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => { await this.ShowPhoto(); });
                }, TimeSpan.FromSeconds(10));
        }

        private async Task ShowPhoto()
        {
            var bitmap = await this.GetBitmapImage();
            this.ImageSource = bitmap;
        }

        private async Task LoadImages()
        {
            await this.photoLibrary.Load();
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

            await bitmapImage.SetSourceAsync(stream);

            return bitmapImage;
        }

        private StorageFile PickNextPhoto()
        {
            var pickFromNewPhotos = this.random.Next()%2 == 1;
            if (pickFromNewPhotos)
            {
                return this.GetRandomPhoto(this.photoLibrary.NewPhotos);
            }

            return this.GetRandomPhoto(this.photoLibrary.Photos);
        }

        private StorageFile GetRandomPhoto(IReadOnlyList<StorageFile> photos)
        {
            return photos[this.random.Next()%photos.Count];
        }
    }
}