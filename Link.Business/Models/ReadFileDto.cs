using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Models
{
    public class ReadFileDto
    {
        public byte[] bytes { get; set; }
        public string fileName { get; set; }
        public string contentType { get; set; }
    }
}
