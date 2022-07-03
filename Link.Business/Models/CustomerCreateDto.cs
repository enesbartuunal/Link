using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Models
{
    public class CustomerCreateDto
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Email { get; set; }
        public IFormFile Image { get; set; }
        public string Phone { get; set; }
        public string Şehir { get; set; }
    }
}
