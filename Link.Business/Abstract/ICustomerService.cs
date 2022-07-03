using Link.Business.Models;
using Link.Core.Utilities.Results;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Abstract
{
    public interface ICustomerService
    {
        Task<Result<string>> AddFile(IFormFile file);
        Task<Result<IList<ReadFileDto>>> GetFiles(string[] fileNames);
        Task<Result<ReadFileDto>> GetFile( string fileName);
    }
}
