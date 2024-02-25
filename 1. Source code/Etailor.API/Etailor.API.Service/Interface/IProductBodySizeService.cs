using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IProductBodySizeService
    {
        Task<bool> CreateProductBodySize(string productId, string templateId, string profileId, string cusId);
        bool UpdateSingle(ProductBodySize productBodySize);
    }
}
