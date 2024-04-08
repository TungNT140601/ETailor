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
        Task<bool> CreateProfileBody(string customerId, string? staffId, string name, List<(string id, decimal? value)> bodySizeId);
        Task<bool> UpdateProfileBody(string customerId, string? staffId, string name, string profileBodyId, List<BodyAttribute>? bodyAttributes, ProfileBody profileBody);
        Task<bool> DeleteProfileBody(string id);

        Task<ProfileBody> GetProfileBody(string id);

        IEnumerable<ProfileBody> GetProfileBodysByCustomerId(string customerId);

        IEnumerable<ProfileBody> GetProfileBodysByStaffId(string? search);
    }
}
