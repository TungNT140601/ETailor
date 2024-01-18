using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IStaffService
    {
        Staff CheckLogin(string username, string password);
        bool AddNewStaff(Staff staff);
        void Logout(string id);
        Staff GetStaff(string id);
    }
}
