using Link.Business.Models;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.WatermarkWorker
{
    public class WaterMarkService : IConsumer<WaterMarkDto>
    {       
        public Task Consume(ConsumeContext<WaterMarkDto> context)
        {
            var path = Path.Combine(context.Message.hostAdress, "wwwroot/Uploads/" + context.Message.fileName);
           using var img=Image.FromFile(path);
            var siteName = "www.mysite.com";
            using var  graphic= Graphics.FromImage(img);
            var font = new Font(FontFamily.GenericSerif, 32, FontStyle.Bold, GraphicsUnit.Pixel);
            var textSize = graphic.MeasureString(siteName, font);
            var color = Color.FromArgb(128, 255, 255, 255);
            var brush=new SolidBrush(color);
            var position = new Point(img.Width-((int)textSize.Width+30),img.Height-((int)textSize.Height+30));

            graphic.DrawString(siteName, font, brush, position);
            var resultPath = Path.Combine(context.Message.hostAdress, "wwwroot/Watermark/" + context.Message.fileName);
            img.Save(resultPath);
            img.Dispose();
            graphic.Dispose();
            return Task.CompletedTask;
              
        }
    }
}
