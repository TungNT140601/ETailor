using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IBodySizeService
    {
        Task<bool> CreateBodySize(BodySize bodySize, string wwwroot, IFormFile? image);

        Task<bool> UpdateBodySize(BodySize bodySize, string wwwroot, IFormFile? image);

        bool DeleteBodySize(string id);

        Task<BodySize> GetBodySize(string id);

        Task<IEnumerable<BodySize>> GetBodySizes(string? search);
    }
}
