using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas.Effects;

namespace Slideshow
{
    internal class Gallery
    {
        private readonly GaussianBlurEffect blurEffect;
        private readonly Compositor compositor;
        private readonly CoreDispatcher dispatcher;
        private readonly Image image;
        private readonly Image background;
        private readonly Grid mainGrid;
        private readonly PhotoLibrary photoLibrary;
        private readonly ProgressRing progressRing;
        private readonly Random random = new Random();
        private IList<StorageFile> newPhotos;
        private IList<StorageFile> photos;

        public Gallery(CoreDispatcher dispatcher, PhotoLibrary photoLibrary, Image image, Image background, ProgressRing progressRing)
        {
            this.dispatcher = dispatcher;
            this.photoLibrary = photoLibrary;
            this.image = image;
            this.background = background;
            this.progressRing = progressRing;
        }

        public async void Start()
        {
            await this.LoadImages();

            ThreadPoolTimer.CreatePeriodicTimer(
                async (source) =>
                {
                    await this.dispatcher.RunAsync(CoreDispatcherPriority.High,
                        async () =>
                        {
                            var bitmap = await this.GetBitmapImage();
                            this.image.Source = bitmap;
                            this.background.Source = bitmap;
                        });
                }, TimeSpan.FromSeconds(10));

            ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await this.dispatcher.RunAsync(CoreDispatcherPriority.High,
                    async () => { await this.LoadImages(); });
            }, TimeSpan.FromHours(1));
        }

        private async Task LoadImages()
        {
            this.progressRing.IsActive = true;
            this.progressRing.Visibility = Visibility.Visible;

            var photoTask = await this.photoLibrary.GetAllPhotos();
            this.photos = photoTask.ToList();
            this.newPhotos = this.PickNewPhotos();

            this.progressRing.Visibility = Visibility.Collapsed;
            this.progressRing.IsActive = false;
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