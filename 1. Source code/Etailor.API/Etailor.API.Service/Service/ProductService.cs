using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
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

        public async Task<bool> AddProduct(Product product)
        {
            var checkDuplicateId = Task.Run(() =>
            {
                if (productRepository.GetAll(x => x.Id == product.Id && x.IsActive == true).Any())
                {
                    throw new UserException("Mã Id Product đã được sử dụng");
                }
            });
            var setValue = Task.Run(() =>
            {
                product.Id = Ultils.GenGuidString();
                product.CreatedTime = DateTime.Now;
                product.LastestUpdatedTime = DateTime.Now;
                product.InactiveTime = null;
                product.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            return productRepository.Create(product);
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            var dbProduct = productRepository.Get(product.Id);
            if (dbProduct != null && dbProduct.IsActive == true)
            {
                var setValue = Task.Run(() =>
                {
                    dbProduct.ProductTemplateId = product.ProductTemplateId;
                    dbProduct.Name = product.Name;
                    dbProduct.Note = product.Note;
                    dbProduct.Status = product.Status;
                    dbProduct.EvidenceImage = product.EvidenceImage;
                    //dbProduct.FinishTime = DateTime.Now;

                    dbProduct.CreatedTime = null;
                    dbProduct.LastestUpdatedTime = DateTime.Now;
                    dbProduct.InactiveTime = null;
                    dbProduct.IsActive = true;
                });

                await Task.WhenAll(setValue);

                return productRepository.Update(dbProduct.Id, dbProduct);
            }
            else
            {
                throw new UserException("Không tìm thấy sản phầm");
            }
        }

        public async Task<bool> DeleteProduct(string id)
        {
            var dbProduct = productRepository.Get(id);
            if (dbProduct != null && dbProduct.IsActive == true)
            {
                var checkChild = Task.Run(() =>
                {
                    //if (productTemplateRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any() || componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
                    //{
                    //    throw new UserException("Không thể xóa danh mục sản phầm này do vẫn còn các mẫu sản phẩm và các loại thành phần sản phẩm vẫn còn thuộc danh mục này");
                    //}
                });
                var setValue = Task.Run(() =>
                {
                    dbProduct.CreatedTime = null;
                    dbProduct.LastestUpdatedTime = DateTime.Now;
                    dbProduct.IsActive = false;
                    dbProduct.InactiveTime = DateTime.Now;
                });

                await Task.WhenAll(checkChild, setValue);

                return productRepository.Update(dbProduct.Id, dbProduct);
            }
            else
            {
                throw new UserException("Không tìm thấy danh mục sản phầm");
            }
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
