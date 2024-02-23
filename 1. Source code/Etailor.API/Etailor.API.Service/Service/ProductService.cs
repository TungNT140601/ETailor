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
        private readonly ITemplateStateRepository templateStateRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentStageRepository componentStageRepository;
        private readonly IProductStageRepository productStageRepository;

        public ProductService(IProductRepository productRepository, IProductTemplateRepository productTemplateRepository,
            IOrderRepository orderRepository, ITemplateStateRepository templateStateRepository, IComponentTypeRepository componentTypeRepository,
            IComponentRepository componentRepository, IComponentStageRepository componentStageRepository, IProductStageRepository productStageRepository)
        {
            this.productRepository = productRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.orderRepository = orderRepository;
            this.templateStateRepository = templateStateRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.componentRepository = componentRepository;
            this.componentStageRepository = componentStageRepository;
            this.productStageRepository = productStageRepository;
        }

        public async Task<bool> AddProduct(string orderId, Product product, List<ProductComponent> productComponents)
        {
            var order = orderRepository.Get(orderId);
            if (order != null)
            {
                var tasks = new List<Task>();

                product.Id = Ultils.GenGuidString();

                if (product.ProductTemplateId == null)
                {
                    throw new UserException("Vui lòng chọn bản mẫu cho sản phẩm");
                }

                var template = productTemplateRepository.Get(product.ProductTemplateId);

                if (template == null || template.IsActive == false)
                {
                    throw new UserException("Không tìm thấy bản mẫu");
                }

                var templateStages = templateStateRepository.GetAll(x => x.ProductTemplateId == product.ProductTemplateId && x.IsActive == true).ToList();

                var componentStages = componentStageRepository.GetAll(x => templateStages.Select(t => t.Id).Contains(x.TemplateStageId)).ToList();

                var templateComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true).ToList();

                var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == template.Id && x.IsActive == true).ToList();

                var productStages = new List<ProductStage>();

                #region InitProduct
                tasks.Add(Task.Run(() =>
                        {
                            if (templateStages != null && templateStages.Any())
                            {
                                productStages = templateStages.Select(x => new ProductStage()
                                {
                                    Id = Ultils.GenGuidString(),
                                    InactiveTime = null,
                                    Deadline = null,
                                    StartTime = null,
                                    FinishTime = null,
                                    IsActive = true,
                                    ProductId = product.Id,
                                    StageNum = x.StageNum,
                                    TaskIndex = 0,
                                    StaffId = null,
                                    TemplateStageId = x.Id,
                                    Status = 1
                                }).ToList();
                            }
                        }));

                tasks.Add(Task.Run(() =>
                {
                    product.OrderId = orderId;
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(product.Name))
                    {
                        product.Name = template.Name;
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(product.Note))
                    {
                        product.Note = "";
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.Price = template.Price;
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.Status = 1;
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.EvidenceImage = "";
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.FinishTime = null;
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.CreatedTime = DateTime.Now;
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.LastestUpdatedTime = DateTime.Now;
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.InactiveTime = null;
                }));

                tasks.Add(Task.Run(() =>
                {
                    product.IsActive = true;
                }));
                #endregion

                await Task.WhenAll(tasks);

                if (productRepository.Create(product))
                {
                    if (productStages.Any())
                    {
                        if (productStageRepository.CreateRange(productStages))
                        {

                        }
                        else
                        {
                            throw new SystemsException("Thêm quá trình của sản phẩm thất bại");
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    throw new SystemsException("Thêm sản phẩm vào hóa đơn thất bại");
                }
                return false;
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
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
