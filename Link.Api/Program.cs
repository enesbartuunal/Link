using Link.Business.Concrete;
using Link.Business.Jobs;
using Link.DataAccess.Context;
using Link.DataAccess.Entities;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(x => x.LowercaseUrls = true);
builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseNpgsql(
                   builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
})
              .AddEntityFrameworkStores<AppDbContext>()
              .AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:Key"])),
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidAudience = builder.Configuration["Token:Audience"],
        ValidIssuer = builder.Configuration["Token:Issuer"],
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole",
         policy => policy.RequireRole("Admin"));
});
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AutService>();
builder.Services.AddScoped<CustomerActivityService>();
builder.Services.AddScoped<ReportService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddQuartz(q =>
{
    q.AddJob<HowManyCustomersInWhichCityJob>(x => x.WithIdentity("HowManyCustomersInWhichCityJob"));
    q.AddTrigger(x => x.ForJob("HowManyCustomersInWhichCityJob").WithIdentity("HowManyCustomersInWhichCityJob-trigger").WithCronSchedule("00 09 30 * *"));

    q.AddJob<HowManyCustomersInWhichCityJob>(x => x.WithIdentity("WeeklyReportJob"));
    q.AddTrigger(x => x.ForJob("WeeklyReportJob").WithIdentity("WeeklyReportJob-trigger").WithCronSchedule("0 09 * * 1"));

    q.AddJob<HowManyCustomersInWhichCityJob>(x => x.WithIdentity("SamePhoneNumbersJob"));
    q.AddTrigger(x => x.ForJob("SamePhoneNumbersJob").WithIdentity("SamePhoneNumbersJob-trigger").WithCronSchedule("0 09 * * 1"));

    q.UseMicrosoftDependencyInjectionJobFactory();
    // base quartz scheduler, job and trigger configuration
});
// ASP.NET Core hosting
builder.Services.AddQuartzHostedService(options =>
   // when shutting down we want jobs to complete gracefully
   options.WaitForJobsToComplete = true
);

builder.Services.AddMassTransit(x =>
{

    x.UsingRabbitMq((content, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQUrl"], "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });
    });

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

