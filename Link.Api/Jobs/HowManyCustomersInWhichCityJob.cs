using ClosedXML.Report;
using Link.Core.Utilities.Email;
using Link.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Globalization;

namespace Link.Business.Jobs
{
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

            var list =  await _appDbContext.HowManyCustomersInWhichCityModels.FromSqlRaw("").ToListAsync();
            if (list.Count> 0)
            {
                var path = Path.Combine(_webHostEnvironmet.ContentRootPath,
                       @"wwwroot/Reports/SehirlereGöreMusterilerRaporu.xlsx");
                var template = new XLTemplate(path);
                template.AddVariable(list);
                template.Generate();

                await using var ms = new MemoryStream();
                template.SaveAs(ms);
                var data = ms.ToArray();
                string[] sendMail = { };
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
}
