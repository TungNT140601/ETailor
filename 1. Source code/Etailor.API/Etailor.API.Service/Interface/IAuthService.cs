using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IAuthService
    {
        Task<bool> Regis(string? username, string? email, string? fullname, string? phone, string? address, string password, IFormFile? avatarFile);
        Customer CheckLoginCus(string? usernameOrEmail, string? password);
        Staff CheckLoginStaff(string? usernameOrEmail, string? password);
    }
}
