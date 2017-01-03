﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Slideshow.Core
{
    internal class PhotoLibrary
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