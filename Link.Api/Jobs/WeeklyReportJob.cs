using ClosedXML.Report;
using Link.Core.Utilities.Email;
using Link.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Globalization;

namespace Link.Api.Jobs
{
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
}

