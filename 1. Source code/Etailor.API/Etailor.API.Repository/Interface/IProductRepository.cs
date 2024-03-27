using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Interface
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task UpdateRangeProduct(List<Product> products);
    }
}
