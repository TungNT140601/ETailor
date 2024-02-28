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
        Task<string> AddProduct(string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity);

        Task<string> UpdateProduct(string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity);

        Task<bool> DeleteProduct(string id);

        Product GetProduct(string id);

        IEnumerable<Product> GetProductsByOrderId(string? orderId);
        IEnumerable<Product> GetProductsByOrderIdOfCus(string? orderId, string cusId);
    }
}
