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
        Task<Product> GetProductOrder(string id, string orderId);
        Task<Product> GetProductOrderByCus(string id, string orderId, string cusId);
        Task<IEnumerable<Product>> GetProductsByOrderId(string orderId);
        Task<IEnumerable<Product>> GetProductsByOrderIds(List<string> orderIds);
        Task<IEnumerable<Product>> GetProductsByOrderIdOfCus(string orderId, string cusId);
        void AutoCreateEmptyTaskProduct();
        Product GetProduct(string id);
        Task AssignTaskToStaff(string productId, string? staffId, int? index);
        void ResetIndex(string? staffId);
    }
}
