using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Setre.Titan.Core.Utilities.Images
{
    public class ImageWriter : IImageWriter
    {
        public readonly IHostingEnvironment _environment;

        public ImageWriter(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadImage(IFormFile file)
        {
            if (CheckIfImageFile(file))
            {
                return await WriteFile(file);
            }

            return "Invalid image file";
        }

        public async Task<string> UploadVideo(IFormFile file)
        {
            return await WriteVideoFile(file);
        }

        /// <summary>
        /// Method to check if file is image file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool CheckIfImageFile(IFormFile file)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return WriterHelper.GetImageFormat(fileBytes) != WriterHelper.ImageFormat.unknown;
        }

        /// <summary>
        /// Method to write file onto the disk
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<string> WriteFile(IFormFile file)
        {
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                //if (extension == ".blob") extension = ".jpg";
                fileName = Guid.NewGuid() + extension;
                var path = Path.Combine(_environment.ContentRootPath, "wwwroot/Uploads/" + fileName);
                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return fileName;
        }


        private async Task<string> WriteVideoFile(IFormFile file)
        {
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = Guid.NewGuid() + extension;
                var path = Path.Combine(_environment.ContentRootPath, "wwwroot/Uploads/" + fileName);
                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return fileName;
        }
    }
}