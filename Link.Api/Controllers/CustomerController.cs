using Link.Business.Abstract;
using Link.Business.Concrete;
using Link.Business.Models;
using Link.Core.Utilities.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Link.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/<CustomerController>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customerResponceList = new List<CustomerResponceDto>();
                var customerList = _customerService.Get();
                foreach (var item in customerList.Result.Data)
                {
                    var image = await _customerService.GetFile(item.ImagePath);
                    var file = File(image.Data.bytes, image.Data.contentType, Path.GetFileName(image.Data.fileName));
                    var member = new CustomerResponceDto()
                    {
                        CustomerId = item.CustomerId,
                        Email = item.Email,
                        Name = item.Name,
                        Phone = item.Phone,
                        SurName = item.SurName,
                        City = item.City,
                        Image = file,
                    };
                    customerResponceList.Add(member);
                }

                return Ok(customerResponceList);
            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        // GET api/<CustomerController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id is null)
                return BadRequest(new Result<IActionResult>(false, ResultConstant.IdNotNull));
            var data = await _customerService.GetById((int)id);
            if (data != null)
            {
                var image = await _customerService.GetFile(data.Data.ImagePath);
                var file = File(image.Data.bytes, image.Data.contentType, Path.GetFileName(image.Data.fileName));
                var member = new CustomerResponceDto()
                {
                    CustomerId = data.Data.CustomerId,
                    Email = data.Data.Email,
                    Name = data.Data.Name,
                    Phone = data.Data.Phone,
                    SurName = data.Data.SurName,
                    City = data.Data.City,
                    Image = file,
                };
                return Ok(member);
            }
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordNotFound));
        }

        public async Task<IActionResult> Get([FromQuery] int id)
        {
            var data = await _customerService.GetById((int)id);
            if (data != null)
            {
                var image = await _customerService.GetFile(data.Data.ImagePath);
                var file = File(image.Data.bytes, image.Data.contentType, Path.GetFileName(image.Data.fileName));
                var member = new CustomerResponceDto()
                {
                    CustomerId = data.Data.CustomerId,
                    Email = data.Data.Email,
                    Name = data.Data.Name,
                    Phone = data.Data.Phone,
                    SurName = data.Data.SurName,
                    City = data.Data.City,
                    Image = file,
                };
                return Ok(member);
            }
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordNotFound));
        }

        // POST api/<CustomerController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerCreateDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.AddFile(model.Image);
                if (result.IsSuccess)
                {
                    var data = new CustomerDto()
                    {
                        CustomerId = model.CustomerId,
                        Name = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        SurName = model.SurName,
                        City = model.City,
                        ImagePath = result.Data,
                    };
                    var result_2 = await _customerService.Add(data);
                    if (result_2.IsSuccess)
                        return StatusCode(201);

                }
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordCreateNotSuccessfully));
            }
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordCreateNotSuccessfully));
        }

        // PUT api/<CustomerController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CustomerCreateDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.AddFile(model.Image);
                if (result.IsSuccess)
                {
                    var data = new CustomerDto()
                    {
                        CustomerId = id,
                        Name = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        SurName = model.SurName,
                        City = model.City,
                        ImagePath = result.Data,
                    };
                    var result_2 = await _customerService.Update(data);
                    if (result_2.IsSuccess)
                        return Ok(new Result<IActionResult>(true, ResultConstant.RecordUpdateSuccessfully));

                }
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordUpdateNotSuccessfully));
            }
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordUpdateNotSuccessfully));




        }

        // DELETE api/<CustomerController>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return BadRequest(new Result<IActionResult>(false, ResultConstant.IdNotNull));
            var data = await _customerService.Delete((int)id);
            if (data.IsSuccess)
                return Ok(new Result<IActionResult>(true, ResultConstant.RecordRemoveSuccessfully));
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordRemoveNotSuccessfully));
        }
    }
}
