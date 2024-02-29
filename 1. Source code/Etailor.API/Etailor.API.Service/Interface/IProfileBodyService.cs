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
        Task<bool> CreateProfileBodyByStaff(string customerId, string staffId, string name, List<(string id, decimal value)> bodySizeId);

        Task<bool> CreateProfileBodyByCustomer(string customerId, string name, List<(string id, decimal value)> bodySizeId);

        Task<bool> UpdateProfileBody(ProfileBody ProfileBody);
        Task<bool> UpdateProfileBodyByStaff(string customerId, string staffId, string name, string profileBodyId, List<(string id, decimal value)> bodySizeId);
        Task<bool> UpdateProfileBodyByCustomer(string customerId, string name, string profileBodyId, List<(string id, decimal value)> bodySizeId);

        Task<bool> DeleteProfileBody(string id);

        //Task<ProfileBody> GetProfileBody(string id);

        ProfileBody GetProfileBody(string id);

        IEnumerable<ProfileBody> GetProfileBodysOfCustomerId(string? search);
        IEnumerable<ProfileBody> GetProfileBodysByCustomerId(string? search);

        IEnumerable<ProfileBody> GetProfileBodysByStaffId(string? search);
    }
}
