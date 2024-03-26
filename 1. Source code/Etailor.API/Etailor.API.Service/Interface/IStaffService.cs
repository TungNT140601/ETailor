using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
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
        Task<bool> AddNewStaff(Staff staff, string wwwroot, IFormFile? avatar, List<string>? masterySkills);
        void Logout(string id);
        Task<Staff> GetStaff(string id);
        Task<Staff> GetStaffInfo(string id);
        bool CheckSecrectKey(string id, string key);
        Task<bool> UpdateInfo(Staff staff, string wwwroot, IFormFile? avatar, List<string>? masterySkills, string role);
        Task<bool> ChangePass(string id, string? oldPassword, string newPassword, string role);
        IEnumerable<Staff> GetAll(string? search);
        IEnumerable<Staff> GetAllWithPagination(string? search, int? pageIndex, int? itemPerPage);
        bool RemoveStaff(string staffId);
    }
}
