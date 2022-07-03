using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Models
{
    public class CustomerActivityDto
    {
        public int CustomerActivityID { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public DateTime Date { get; set; }
    }
}
