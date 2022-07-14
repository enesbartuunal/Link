using Link.Core.Entities;
using Link.Core.Utilities.IoC;
using Link.DataAccess.Entities;
using Link.DataAccess.Entities.ForJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Link.DataAccess.Context
{
    public class AppDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<User>
    {
        private IHttpContextAccessor _httpContextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerActivity> CustomorActivities { get; set; }

        public DbSet<Report> Reports { get; set; }


        //Migration sırasında yorum satırı olması gereken proplar
        //public DbSet<HowManyCustomersInWhichCityModel> HowManyCustomersInWhichCityModels { get; set; }
        public override int SaveChanges()
        {
            _httpContextAccessor = ServiceTool.ServiceProvider.GetService<IHttpContextAccessor>();
            int? userId = null;
            if (_httpContextAccessor.HttpContext?.User.Identity.IsAuthenticated != null &&
                (bool)_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                userId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                    .Value);

            var entities = ChangeTracker.Entries().Where(x =>
                x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified ||
                                           x.State == EntityState.Deleted));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).IsActive = true;
                    ((BaseEntity)entity.Entity).IsDeleted = false;
                    ((BaseEntity)entity.Entity).CreatedDate = DateTime.Now;
                    ((BaseEntity)entity.Entity).CreatedBy = userId;
                }
                else if (entity.State == EntityState.Deleted)
                {
                    entity.State = EntityState.Modified;
                    ((BaseEntity)entity.Entity).DeletedDate = DateTime.Now;
                    ((BaseEntity)entity.Entity).DeletedBy = userId;
                    ((BaseEntity)entity.Entity).IsDeleted = true;
                    ((BaseEntity)entity.Entity).IsActive = false;
                }

                ((BaseEntity)entity.Entity).UpdatedDate = DateTime.Now;
                ((BaseEntity)entity.Entity).UpdatedBy = userId;
            }

            return base.SaveChanges();
        }
    }
}
