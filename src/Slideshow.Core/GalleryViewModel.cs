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

        private IList<StorageFile> newPhotos;
        private IList<StorageFile> photos;

        public GalleryViewModel(CoreDispatcher dispatcher)
        {
            this.photoLibrary = new PhotoLibrary();
            this.dispatcher = dispatcher;
            this.Start();
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

            ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await this.dispatcher.RunAsync(CoreDispatcherPriority.High,
                    async () => { await this.LoadImages(); });
            }, TimeSpan.FromHours(1));
        }

        private async Task ShowPhoto()
        {
            var bitmap = await this.GetBitmapImage();
            this.ImageSource = bitmap;
        }

        private async Task LoadImages()
        {
            var photoTask = await this.photoLibrary.GetAllPhotos();
            this.photos = photoTask.ToList();
            this.newPhotos = this.PickNewPhotos();
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