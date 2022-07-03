using Link.Business.Abstract;
using Link.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Link.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutController : ControllerBase
    {
        private readonly IAutService _autService;

        public AutController(IAutService autService)
        {
            _autService = autService;
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] RegisterDto model)
        {
            var result = await _autService.Register(model);
            return result.IsSuccess ? StatusCode(201) : BadRequest(result);
        }
        [HttpPost("signin")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] LoginDto model)
        {
            var result = await _autService.Login(model);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result);
        }
    }
}
