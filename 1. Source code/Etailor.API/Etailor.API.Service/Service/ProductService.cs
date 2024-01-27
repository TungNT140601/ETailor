using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository productRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly IOrderRepository orderRepository;

        public ProductService(IProductRepository productRepository, IProductTemplateRepository productTemplateRepository, IOrderRepository orderRepository)
        {
            this.productRepository = productRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.orderRepository = orderRepository;
        }

        public Product GetProduct(string id)
        {
            var product = productRepository.Get(id);
            return product == null ? null : product.IsActive == true ? product : null;
        }

        public IEnumerable<Product> GetProductsByOrderId(string? search)
        {
            return productRepository.GetAll(x => ((search != null && x.OrderId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
