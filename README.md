# Proje Mimarisi Hakkında

------------

> **Core**
                
	1. Bu katmanda projede kullandığım araçlar bulunur(Email,Extensions,vs..) 
               
> **DataAccess**
                
	1.Veri tabanı CodeFirst yaklaşımı ile oluşturulmus olup, bu katmanda veri aktarım işlemleri yapılmıştır.
               

> **Business**
                
	1.Projede kullandığım tüm servisler bu katmandadır.
	2.Mapleme işlemleri,Crud erişimi  ve projede kullandığım modeller bu katmandadır.
	
              

# Firma Beklentileri Hakında

------------
=>**Müşterinin isim,soy isim,email,fotograf,telefon ve şehir  bilgilerinin tutulması**
=>**Müşteri ile yapılan ticari faliyet bilgilerinin tutulması**

```csharp
namespace Link.DataAccess.Entities
{
    public class CustomerActivity
    {
        public int CustomerActivityID { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public DateTime Date { get; set; }

        //Relations
        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }
    }

	 public class Customer
   	 {
		public int CustomerID { get; set; }
       		public string Name { get; set; }
       		public string SurName { get; set; }
      	  	public string Email { get; set; }
     	  	public string ImagePath { get; set; }
		public string Phone { get; set; }
      	 	public string City { get; set; }

        	//Relations
       		public ICollection<CustomerActivity> CustomerActivities { get; set; }
   	 }
	}
```
=>**Rehbere, müşteri kaydetme,güncelleme,silme,listeleme,fotoğraf güncelleme,toplu silme işlemi ve müşteri tiraci faaliyetler ile ilgili CRUD operasyonlarının gerçekleştirilmesi.**
 + Bu işlemler  Business katmanları içerisinde yapılamaktadır.
    + Crud işlemleri için Base klosorü içerisinde servis oluşturuldu.
    + Crud işlemleri dışında CustomerService'e farklı özellikler kazandırıldı.(AddFile,GetFile)
    
    ```csharp
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
	```

=>**Rehbere kayıt gerçekleştirecek çalışanların farklı rollere sahip olması( Admin ve Editor Rolleri)**
=>**Admin,Editor isminde 2 tane rol tanımlamasının olması.**

+ Bu işlem için Core katmanında Extension hazırlandı.


    ```csharp
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
	```
=>**	Admin rol, tüm işlemleri gerçekleştirebilecek.
=>Editor rol  ise sadece kaydetme,güncelleme ve listeleme işlemlerini gerçekleştirebilecek.
**
+ Bu işlem için bir Policy oluşturuldu ve metotlara bu policy  tanımlandı.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole",
         policy => policy.RequireRole("Admin"));
});
```

```csharp
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
```

------------


=>**Kaydedilen müşteri fotoğraflarına watermark(filigran) eklenecektir.**
+ Bu işlem için bir worker service oluşturuldu.Bağımsız çalışma için MassTransit kullanıldı.RabbitMq MassTransit kütüphanesi  kullanılarak resimlere watermark eklenip tekrar wwwRoot/Watermark adresine kayıt işlemi gerçekleştirildi.

=>**Aynı telefon numarasına sahip ama farklı isim ile kaydedilmiş müşterilerin tesbit edilmesi.**
=>**Aylık olarak email yoluyla  hangi şehirde kaç tane müşteri olduğu raporlancak.**
=>**Haftalık olarak en fazla tiraci faliyete sahip ilk 5  müşteri  email yoluyla raporlanacak. Bu rapor admin rolune sahip olanlara =>gönderilecektir.**
=>**Rapor'lar excel formatında olacak**
	+  Raporlamada Quartz kütüphanesi kullanıldı.
	+Raporlama sureleri Program.cs altında Crone ile tanımlandı.
	+ Api katmanında Job klosörü altına istenilen tüm joblar tanımlandı.
	+ Joblar sonuçları excel formatında olup, excel dönüşümü için ClosedXML kütüphanesi kullanıldı.
	+ Mail gönderimi için MailKit kütüphanesi kullanıldı.
	+ İstenilen raporlar Linq kulllanılarak oluşturuldu.Burada StoreProsedurlardanda faydanılabilirdi.
	+ Gönderim mail adresleri admin gönderimleri için farklı genel için farklıdır.
	+ Gönderen adresi appsetting.json dosyasında tanımlanmalıdır.Ayrıca mailer ayarları core katmanında isteğe göre override edilebilir.
------------
```csharp
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
```

```csharp
public class HowManyCustomersInWhichCityJob : IJob
    {
        private readonly AppDbContext _appDbContext;
        private readonly IWebHostEnvironment _webHostEnvironmet;
        private readonly IMailer _mailer;
        public HowManyCustomersInWhichCityJob(AppDbContext appDbContext, IWebHostEnvironment webHostEnvironmet, IMailer mailer)
        {
            _webHostEnvironmet = webHostEnvironmet;
            _mailer = mailer;
            _appDbContext = appDbContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {

            var list = await _appDbContext.Customers.GroupBy(x => x.City).Select(y => new { City = y.Key, CustomerCount = y.Count() }).ToListAsync();
            if (list.Count > 0)
            {
                var path = Path.Combine(_webHostEnvironmet.ContentRootPath,
                       @"wwwroot/Reports/SehirlereGöreMusterilerRaporu.xlsx");
                var template = new XLTemplate(path);
                template.AddVariable(list);
                template.Generate();

                await using var ms = new MemoryStream();
                template.SaveAs(ms);
                var data = ms.ToArray();
                string[] sendMail = _appDbContext.Users.Select(x => x.Email).ToArray();
                string[] cc = { };

                var att = new List<MailAttachments>
                            {
                                new MailAttachments
                                {
                                    File = data,
                                    FileName = "SehirlereGöreMusterilerRaporu " +
                                               DateTime.Now.ToString("dd/MM/yyyy", new CultureInfo("tr-TR")) + ".xlsx"
                                }
                            };
                var body = string.Empty;
                _mailer.SendEmail(sendMail, cc, "SehirlereGöreMusterilerRaporu ", body, att);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
```

```csharp
  public class WeeklyReportJob : IJob
    {
        private readonly AppDbContext _appDbContext;
        private readonly IWebHostEnvironment _webHostEnvironmet;
        private readonly IMailer _mailer;

        public WeeklyReportJob(AppDbContext appDbContext, IWebHostEnvironment webHostEnvironmet, IMailer mailer)
        {
            _appDbContext = appDbContext;
            _webHostEnvironmet = webHostEnvironmet;
            _mailer = mailer;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var list = await _appDbContext.CustomorActivities.GroupBy(x => x.CustomerID).Select(y => new { CustomerID = y.Key, CustomerActivityCount = y.Count(),CustomerTotalCost=y.Sum(z=>z.Cost)}).OrderBy(x=>x.CustomerActivityCount).Take(5).ToListAsync();
            if (list.Count > 0)
            {
                var path = Path.Combine(_webHostEnvironmet.ContentRootPath,
                       @"wwwroot/Reports/HaftalıkRapor.xlsx");
                var template = new XLTemplate(path);
                template.AddVariable(list);
                template.Generate();

                await using var ms = new MemoryStream();
                template.SaveAs(ms);
                var data = ms.ToArray();
                var adminUsers = _appDbContext.UserRoles.Where(x => x.RoleId == "1");
                string[] sendMail = _appDbContext.Users.Where(x=>adminUsers.Any(y=>y.UserId==x.Id)).Select(x => x.Email).ToArray();
                string[] cc = { };

                var att = new List<MailAttachments>
                            {
                                new MailAttachments
                                {
                                    File = data,
                                    FileName = "HaftalıkRapor " +
                                               DateTime.Now.ToString("dd/MM/yyyy", new CultureInfo("tr-TR")) + ".xlsx"
                                }
                            };
                var body = string.Empty;
                _mailer.SendEmail(sendMail, cc, "HaftalıkRapor ", body, att);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
```

```csharp
  public class SamePhoneNumbersJob : IJob
    {
        private readonly AppDbContext _appDbContext;
        private readonly IWebHostEnvironment _webHostEnvironmet;
        private readonly IMailer _mailer;

        public SamePhoneNumbersJob(AppDbContext appDbContext, IWebHostEnvironment webHostEnvironmet, IMailer mailer)
        {
            _appDbContext = appDbContext;
            _webHostEnvironmet = webHostEnvironmet;
            _mailer = mailer;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            var list = await _appDbContext.Customers.GroupBy(x => x.Phone).Select(y => new { Phone = y.Key, CustomerCount = y.Count(), CustomerList = y.Select(x=>x.Name).ToList() }).Where(x=>x.CustomerCount>1).ToListAsync();
            if (list.Count > 0)
            {
                var path = Path.Combine(_webHostEnvironmet.ContentRootPath,
                       @"wwwroot/Reports/AyniTelefonNumaralarinaSahipMusteriler.xlsx");
                var template = new XLTemplate(path);
                template.AddVariable(list);
                template.Generate();

                await using var ms = new MemoryStream();
                template.SaveAs(ms);
                var data = ms.ToArray();
                var adminUsers = _appDbContext.UserRoles.Where(x => x.RoleId == "1");
                string[] sendMail = _appDbContext.Users.Where(x => adminUsers.Any(y => y.UserId == x.Id)).Select(x => x.Email).ToArray();
                string[] cc = { };

                var att = new List<MailAttachments>
                            {
                                new MailAttachments
                                {
                                    File = data,
                                    FileName = "AyniTelefonNumaralarinaSahipMusteriler " +
                                               DateTime.Now.ToString("dd/MM/yyyy", new CultureInfo("tr-TR")) + ".xlsx"
                                }
                            };
                var body = string.Empty;
                _mailer.SendEmail(sendMail, cc, "AyniTelefonNumaralarinaSahipMusteriler ", body, att);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
```
=>**	Eski raporlara erişilebilecek ve indirilebilecek.**
 + Report endpointinden eski raporlara erişim sağlanıyor.


