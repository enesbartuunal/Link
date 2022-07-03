using AutoMapper;
using Link.Business.Abstract;
using Link.Business.Base;
using Link.Business.Models;
using Link.DataAccess.Context;
using Link.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Concrete
{
    public class CustomerActivityService : ServiceAbstractBase<CustomerActivity, CustomerActivityDto>, ICustomerActivityService
    {
        public CustomerActivityService(AppDbContext db, IMapper mapper) : base(db, mapper)
        {
        }
    }
}
