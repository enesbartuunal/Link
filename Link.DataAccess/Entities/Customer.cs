using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.DataAccess.Entities
{
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
