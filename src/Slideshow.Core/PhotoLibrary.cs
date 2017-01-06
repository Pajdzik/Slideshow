using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Threading;

namespace Slideshow.Core
{
    internal class PhotoLibrary
    {
        private const int NewPhotosLimit = 50;

        private List<StorageFile> newPhotos;
        private List<StorageFile> allPhotos;

        public IReadOnlyList<StorageFile> Photos => new ReadOnlyCollection<StorageFile>(this.allPhotos);

        public IReadOnlyList<StorageFile> NewPhotos => new ReadOnlyCollection<StorageFile>(this.newPhotos);

        public async Task Load()
        {
            await this.LoadPhotos();

            ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await this.LoadPhotos();
            }, TimeSpan.FromMinutes(10));
        }

        private async Task LoadPhotos()
        {
            this.allPhotos = await this.LoadAllPhotos();
            this.newPhotos = this.LoadNewPhotos(this.allPhotos);
        }

        private async Task<List<StorageFile>> LoadAllPhotos()
        {
            var files =
                await this.GetPhotosFromFolderAndSubfolders(KnownFolders.PicturesLibrary);
            return files;
        }

        private List<StorageFile> LoadNewPhotos(IEnumerable<StorageFile> photos)
        {
            var newPhotos = photos.OrderBy(photo => photo.DateCreated).Take(NewPhotosLimit);
            return newPhotos.ToList();
        }

        private async Task<List<StorageFile>> GetPhotosFromFolderAndSubfolders(StorageFolder folder)
        {
            var files = new List<StorageFile>();

            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                var subFiles = await this.GetPhotosFromFolderAndSubfolders(subFolder);
                files.AddRange(subFiles);
            }

            foreach (var file in await folder.GetFilesAsync())
            {
                if (IsProperImage(file))
                {
                    files.Add(file);
                }
            }

            return files;
        }

        private static bool IsProperImage(StorageFile file)
        {
            return file.IsAvailable && file.ContentType.StartsWith("image");
        }
    }
}