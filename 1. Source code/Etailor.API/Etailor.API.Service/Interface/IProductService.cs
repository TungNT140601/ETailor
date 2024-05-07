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
        Task<string> AddProduct(string wwwroot, string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity, int quantity);

        Task<string> UpdateProduct(string wwwroot, string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity);
        Task<bool> UpdateProductPrice(string orderId, string productId, decimal? price);
        Task<bool> DeleteProduct(string id);
        Task<Product> GetProductOrder(string id, string orderId);
        Task<Product> GetProductOrderByCus(string id, string orderId, string cusId);
        Task<IEnumerable<Product>> GetProductsByOrderId(string orderId);
        Task<IEnumerable<Product>> GetProductsByOrderIds(List<string> orderIds);
        Task<IEnumerable<Product>> GetProductsByOrderIdOfCus(string orderId, string cusId);
        Task<List<ProductBodySize>> GetBodySizeOfProduct(string productId, string orderId, string? cusId);
        Task<IEnumerable<ComponentType>> GetProductComponent(string templateId);
    }
}
