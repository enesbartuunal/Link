using Link.Business.Concrete;
using Link.Business.Models;
using Link.Core.Utilities.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Link.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerActivityController : ControllerBase
    {
        private readonly CustomerActivityService _customerActivityService;

        public CustomerActivityController(CustomerActivityService customerActivityService)
        {
            _customerActivityService = customerActivityService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _customerActivityService.Get();
            return Ok(list.Data);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id is null)
                return BadRequest(new Result<IActionResult>(false, ResultConstant.IdNotNull));
            var data = await _customerActivityService.GetById((int)id);
            if (data != null)
                return Ok(data.Data);
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordNotFound));
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int id)
        {
            var data = await _customerActivityService.GetById((int)id);
            if (data != null)
                return Ok(data.Data);
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordNotFound));
        }

      
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerActivityDto model)
        {
            var data = await _customerActivityService.Add(model);
            if (data.IsSuccess)
                return StatusCode(201);
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordCreateNotSuccessfully));
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CustomerActivityDto model)
        {

            model.CustomerActivityID = id;
            var data = await _customerActivityService.Update(model);
            if (data.IsSuccess)
                return Ok(new Result<IActionResult>(true, ResultConstant.RecordUpdateSuccessfully));
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordUpdateNotSuccessfully));


        }

        // DELETE api/<CategoriesController>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
                return BadRequest(new Result<IActionResult>(false, ResultConstant.IdNotNull));
            var data = await _customerActivityService.Delete((int)id);
            if (data.IsSuccess)
                return Ok(new Result<IActionResult>(true, ResultConstant.RecordRemoveSuccessfully));
            else
                return BadRequest(new Result<IActionResult>(false, ResultConstant.RecordRemoveNotSuccessfully));
        }
    }
}
