using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Core.Utilities.Extensions
{
    public class CreateDefaultUsersAndRolesMiddleware
    {
        private RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public CreateDefaultUsersAndRolesMiddleware(RequestDelegate next,IConfiguration configuration,IServiceProvider serviceProvider)
        {
            _next = next;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }
        public async Task InvokeAsync()
        {
            try
            {
               CreateRolesAndAdmin(_serviceProvider, _configuration);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async void CreateRolesAndAdmin(IServiceProvider serviceProvider, IConfiguration Configuration)
        {
            //initializing custom roles 

            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string[] roleNames = { "Admin", "Editor" };
            Task<IdentityResult> roleResult;

            foreach (var roleName in roleNames)
            {
                Task<bool> roleExist = RoleManager.RoleExistsAsync(roleName);
                roleExist.Wait();
                if (!roleExist.Result)
                {
                    //create the roles and seed them to the database: Question 1
                    roleResult = RoleManager.CreateAsync(new IdentityRole(roleName));
                    roleResult.Wait();
                }
            }
            //Ensure you have these values in your appsettings.json file
            string userPWD = Configuration["AppSettings:UserPassword"];
            Task<IdentityUser> _user = UserManager.FindByEmailAsync(Configuration["AppSettings:UserEmail"]);
            _user.Wait();
            if (_user.Result == null)
            {
                //Here you could create a super user who will maintain the web app
                var poweruser = new IdentityUser();
                poweruser.UserName = Configuration["AppSettings:UserEmail"];
                poweruser.Email = Configuration["AppSettings:UserEmail"];
                Task<IdentityResult> createPowerUser = UserManager.CreateAsync(poweruser, userPWD);
                createPowerUser.Wait();
                if (createPowerUser.Result.Succeeded)
                {
                    //here we tie the new user to the role
                    Task<IdentityResult> newUserRole = UserManager.AddToRoleAsync(poweruser, "Admin");
                    newUserRole.Wait();
                }
            }
        }
    }
       
}
