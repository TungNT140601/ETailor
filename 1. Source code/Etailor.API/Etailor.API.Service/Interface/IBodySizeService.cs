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

        bool UpdateBodySize(BodySize bodySize);

        bool DeleteBodySize(string id);

        BodySize GetBodySize(string id);

        IEnumerable<BodySize> GetBodySizes(string? search);
    }
}
