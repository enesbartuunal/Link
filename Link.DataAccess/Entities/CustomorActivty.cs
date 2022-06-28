using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.DataAccess.Entities
{
    public class CustomorActivity
    {
        public int CustomerActivityID { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public DateTime Date { get; set; }

        //Relations
        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
