using AutoMapper;
using Link.Business.Models;
using Link.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Mapper
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Customer,CustomerDto>();
            CreateMap<CustomerDto,Customer>();
            CreateMap<CustomerActivity,CustomerActivityDto>();
            CreateMap<CustomerActivityDto,CustomerActivity>();

        }
    }
}
