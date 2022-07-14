using AutoMapper;
using Link.Business.Base;
using Link.Business.Models;
using Link.Core.Utilities.Results;
using Link.DataAccess.Context;
using Link.DataAccess.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Concrete
{
    public class ReportService 
    {
        private readonly IHostingEnvironment _environment;

        public ReportService(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<Result<string>> AddReportFile(IFormFile file)
        {
            string fileName;
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            fileName = file.FileName.Substring(0, (file.FileName.Length - extension.Length)) + "_" + Guid.NewGuid().ToString().Substring(0, 6) + extension;
            var path = Path.Combine(_environment.ContentRootPath, "wwwroot/OldReports/" + fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return new Result<string>(true, ResultConstant.RecordCreateSuccessfully, fileName);
        }
     
    }
}
