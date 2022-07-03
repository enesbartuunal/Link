using Link.Business.Models;
using Link.Core.Utilities.Results;
using Link.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.Business.Abstract
{
    public interface IAutService
    {
        Task<Result<LoginResponceDto>> Login(LoginDto loginDto);
        Task<Result<RegisterResponceDto>> Register(RegisterDto  registerDto);
    }
}
