using Link.Business.Abstract;
using Link.Business.Models;
using Link.Core.Utilities.Results;
using Link.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Concrete
{
    public class AutService : IAutService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AutService(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponceDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            var userRoles = _userManager.GetRolesAsync(user);
            var userRole = "";
            if (userRoles.Result.Contains("Admin"))
                userRole = "Admin";
            else
                userRole = "Member";
            if (user is null)
                return new Result<LoginResponceDto>(false, ResultConstant.InvalidAuthentication);
            var result = await _signInManager.PasswordSignInAsync(user.Email, loginDto.Password, false, true);
            if (result.IsLockedOut)
            {
                return new Result<LoginResponceDto>(false, ResultConstant.LockOut);
            }
            else if (!result.Succeeded)
                return new Result<LoginResponceDto>(false, ResultConstant.CheckPassword);

            var signingCredentials = _tokenService.GetSigningCredentials();
            var claims = await _tokenService.GetClaims(user);
            var tokenOptions = _tokenService.GenerateTokenOptions(signingCredentials, claims);
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _userManager.UpdateAsync(user);
            var returnData = new LoginResponceDto()
            {
                UserName = user.UserName,
                Id = user.Id,
                Email = user.Email,
                Token = new JwtSecurityTokenHandler().WriteToken(tokenOptions),
                RefreshToken = user.RefreshToken
            };
            return new Result<LoginResponceDto>(true,ResultConstant.ValidAuthentication, returnData);
        }

        public async Task<Result<RegisterResponceDto>> Register(RegisterDto registerDto)
        {
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return new Result<RegisterResponceDto>(false, ResultConstant.UserAlreadyExists);
            }
            var user = new User()
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                var last = new RegisterResponceDto();
                last.Errors = errors;
                last.IsSuccessfulRegistration = false;
                return new Result<RegisterResponceDto>(false, ResultConstant.InvalidRegistiration, last);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, ResultConstant.Role_Editor);

            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description);
                var last = new RegisterResponceDto();
                last.Errors = errors;
                last.IsSuccessfulRegistration = false;
                return new Result<RegisterResponceDto>(false, ResultConstant.InvalidRegistiration, last);
            }
            return new Result<RegisterResponceDto>(true, ResultConstant.ValidRegistiration);
        }
    }
}
