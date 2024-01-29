using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IProductService 
    {
        Task<bool> AddProduct(Product Product);

        Task<bool> UpdateProduct(Product Product);

        Task<bool> DeleteProduct(string id);

        Product GetProduct(string id);

        IEnumerable<Product> GetProductsByOrderId(string? search);
    }
}
