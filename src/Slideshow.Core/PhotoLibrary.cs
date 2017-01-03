using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Slideshow.Core
{
    public class PhotoLibrary
    {
        public async Task<IEnumerable<StorageFile>> GetAllPhotos()
        {
            var files = await this.GetPhotosFromFolderAndSubfolders(KnownFolders.PicturesLibrary);
            return files;
        }

        private async Task<IEnumerable<StorageFile>> GetPhotosFromFolderAndSubfolders(StorageFolder folder)
        {
            var files = new List<StorageFile>();

            foreach (var subFolder in await folder.GetFoldersAsync())
            {
                var subFiles = await this.GetPhotosFromFolderAndSubfolders(subFolder);
                files.AddRange(subFiles);
            }

            foreach (var file in await folder.GetFilesAsync())
            {
                if (file.IsAvailable && file.ContentType.StartsWith("image"))
                {
                    files.Add(file);
                }
            }

            return files;
        }
    }
}