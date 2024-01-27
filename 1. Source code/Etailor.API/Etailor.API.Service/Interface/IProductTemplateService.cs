using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IProductTemplateService
    {
        Task<IEnumerable<ProductTemplate>> GetByCategory(string id);
        Task<ProductTemplate> GetByUrlPath(string urlPath);
    }
}
