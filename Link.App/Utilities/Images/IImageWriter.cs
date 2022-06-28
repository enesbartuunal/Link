using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Setre.Titan.Core.Utilities.Images
{
    public interface IImageWriter
    {
        Task<string> UploadImage(IFormFile file);
        Task<string> UploadVideo(IFormFile file);
    }
}
