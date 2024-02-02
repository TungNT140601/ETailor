using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IBodyAttributeService
    {
        Task<bool> AddBodyAttribute(BodyAttribute bodyAttribute);

        Task<bool> UpdateBodyAttribute(BodyAttribute bodyAttribute);

        Task<bool> DeleteBodyAttribute(string id);

        BodyAttribute GetBodyAttribute(string id);

        IEnumerable<BodyAttribute> GetBodyAttributesByProfileBodyId(string? search);

        IEnumerable<BodyAttribute> GetBodyAttributesByBodySizeId(string? search);
    }
}
