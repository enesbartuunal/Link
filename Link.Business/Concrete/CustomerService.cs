using AutoMapper;
using Link.Business.Abstract;
using Link.Business.Base;
using Link.Business.Models;
using Link.Core.Utilities.Images;
using Link.Core.Utilities.Results;
using Link.DataAccess.Context;
using Link.DataAccess.Entities;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Concrete
{
    public class CustomerService : ServiceAbstractBase<Customer, CustomerDto>, ICustomerService
    {
        private readonly IHostingEnvironment _environment;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public CustomerService(AppDbContext db, IMapper mapper, IHostingEnvironment environment,ISendEndpointProvider sendEndpointProvider) : base(db, mapper)
        {
            _environment = environment;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task<Result<string>> AddFile(IFormFile file)
        {
            string fileName;
            try
            {
                if (WriterHelper.CheckIfImageFile(file))
                {
                    var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                    fileName = file.FileName.Substring(0, (file.FileName.Length - extension.Length)) + "_" + Guid.NewGuid().ToString().Substring(0, 6) + extension;
                    var path = Path.Combine(_environment.ContentRootPath, "wwwroot/Uploads/" + fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);
                    //For WaterMark(Worker Service)
                    var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:addwatermark"));
                    var waterMarkDto=new WaterMarkDto() { fileName=fileName,hostAdress=_environment.ContentRootPath};
                    await sendEndpoint.Send<WaterMarkDto>(waterMarkDto);    
                    //
                    return new Result<string>(true, ResultConstant.RecordCreateSuccessfully, fileName);
                }
                return new Result<string>(false, ResultConstant.RecordCreateNotSuccessfully);
            }
            catch (Exception e)
            {
                return new Result<string>(false, e.Message);
            }
        }

        public async Task<Result<IList<ReadFileDto>>> GetFiles(string[] fileNames)
        {
            try
            {
                var resultList = new List<ReadFileDto>();
                foreach (var item in fileNames)
                {
                    var path = Path.Combine(_environment.ContentRootPath, "wwwroot/Uploads/" + item);
                    var provider = new FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(path, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }
                    var bytes = await System.IO.File.ReadAllBytesAsync(path);
                    var result = new ReadFileDto()
                    {
                        bytes = bytes,
                        contentType = contentType,
                        fileName = item
                    };
                    resultList.Add(result);

                }
                return new Result<IList<ReadFileDto>>(true, ResultConstant.RecordFound, resultList);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task<Result<ReadFileDto>> GetFile(string fileName = null)
        {
            try
            {
                var path = Path.Combine(_environment.ContentRootPath, "wwwroot/Uploads/" + fileName);
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(path, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                var result = new ReadFileDto()
                {
                    bytes = bytes,
                    contentType = contentType,
                    fileName = fileName
                };
                return new Result<ReadFileDto>(true, ResultConstant.RecordFound, result);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
