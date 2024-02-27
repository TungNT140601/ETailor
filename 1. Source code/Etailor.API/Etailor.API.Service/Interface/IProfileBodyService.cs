using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IProfileBodyService
    {
        //Task<bool> AddProfileBody(ProfileBody ProfileBody);

        Task<bool> CreateProfileBodyByStaff(string customerId, string staffId, string name, List<(string id, decimal value)> bodySizeId);

        Task<bool> CreateProfileBodyByCustomer(string customerId, string name, List<(string id, decimal value)> bodySizeId);

        Task<bool> UpdateProfileBody(ProfileBody ProfileBody);

        Task<bool> DeleteProfileBody(string id);

        ProfileBody GetProfileBody(string id);

        IEnumerable<ProfileBody> GetProfileBodysByCustomerId(string? search);

        IEnumerable<ProfileBody> GetProfileBodysByStaffId(string? search);
    }
}
