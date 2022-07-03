using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Core.Utilities.Extensions
{
    public static class CreateDefaultUsersAndRolesMiddlewareExtension
    {
        public static void ConfigureCreateDefaultUsersAndRolesMiddleware(this IApplicationBuilder app) { app.UseMiddleware<CreateDefaultUsersAndRolesMiddleware>(); }
    }
}
