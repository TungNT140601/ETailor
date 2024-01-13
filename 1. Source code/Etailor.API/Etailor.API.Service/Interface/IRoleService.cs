using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IRoleService
    {
        IEnumerable<Role> GetRoles();
        string GetRoleId(string roleName);
    }
}
