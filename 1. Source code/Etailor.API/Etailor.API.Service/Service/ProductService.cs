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
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text.Json;
using System.ComponentModel;
using Component = Etailor.API.Repository.EntityModels.Component;

namespace Etailor.API.Service.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductBodySizeService productBodySizeService;
        private readonly IProductRepository productRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly ITemplateStateRepository templateStateRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IComponentStageRepository componentStageRepository;
        private readonly IProductStageRepository productStageRepository;
        private readonly IProductComponentRepository productComponentRepository;
        private readonly IMaterialRepository materialRepository;
        private readonly IProfileBodyRepository profileBodyRepository;
        private readonly IMaterialTypeRepository materialTypeRepository;
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        private readonly IOrderMaterialRepository orderMaterialRepository;

        public ProductService(IProductRepository productRepository, IProductTemplateRepository productTemplateRepository,
            IOrderRepository orderRepository, ITemplateStateRepository templateStateRepository, IComponentTypeRepository componentTypeRepository,
            IComponentRepository componentRepository, IComponentStageRepository componentStageRepository, IProductStageRepository productStageRepository,
            IProductComponentRepository productComponentRepository, IMaterialRepository materialRepository, IProfileBodyRepository profileBodyRepository,
            IProductBodySizeService productBodySizeService, IMaterialTypeRepository materialTypeRepository, IMaterialCategoryRepository materialCategoryRepository,
            IOrderMaterialRepository orderMaterialRepository)
        {
            this.productRepository = productRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.orderRepository = orderRepository;
            this.templateStateRepository = templateStateRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.componentRepository = componentRepository;
            this.componentStageRepository = componentStageRepository;
            this.productStageRepository = productStageRepository;
            this.productComponentRepository = productComponentRepository;
            this.materialRepository = materialRepository;
            this.profileBodyRepository = profileBodyRepository;
            this.productBodySizeService = productBodySizeService;
            this.materialTypeRepository = materialTypeRepository;
            this.materialCategoryRepository = materialCategoryRepository;
            this.orderMaterialRepository = orderMaterialRepository;
        }

        public async Task<string> AddProduct(string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity)
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

                // các bộ phận của bản mẫu
                var templateComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true).ToList();

                // các kiểu bộ phận của bản mẫu
                var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == template.Id && x.IsActive == true).ToList();

                var material = materialRepository.Get(materialId);

                var materialCategory = new MaterialCategory();

                if (material != null)
                {
                    materialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);
                }

                #region InitProduct

                tasks.Add(Task.Run(() =>
                {
                    if (material == null || material.IsActive == false)
                    {
                        throw new UserException("Không tìm thấy nguyên liệu");
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
                    if (isCusMaterial)
                    {
                        product.Price = template.Price;
                    }
                    else
                    {
                        product.Price = template.Price + Math.Abs(Math.Round((decimal)((double)materialCategory.PricePerUnit * materialQuantity), 2));
                    }
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

                //tasks.Add(Task.Run(async () =>
                //{
                var tasksSetComponents = new List<Task>();

                var saveOrderComponents = new List<ProductComponent>();

                if (templateComponentTypes.Any())
                {
                    foreach (var type in templateComponentTypes)
                    {
                        //tasksSetComponents.Add(Task.Run(() =>
                        //{
                        var productComponentAdds = templateComponents.Where(x => x.ComponentTypeId == type.Id && productComponents.Select(c => c.ComponentId).Contains(x.Id));
                        var component = new Component();
                        if (productComponentAdds != null && productComponentAdds.Any())
                        {
                            if (productComponentAdds.Count() > 1)
                            {
                                throw new UserException("Chỉ được chọn 1 kiểu cho 1 bộ phận");
                            }
                            else
                            {
                                component = productComponentAdds.First();
                            }
                        }
                        else
                        {
                            component = templateComponents.SingleOrDefault(x => x.ComponentTypeId == type.Id && x.Default == true);
                        }
                        if (component != null)
                        {
                            saveOrderComponents.Add(new ProductComponent()
                            {
                                ComponentId = component.Id,
                                Id = Ultils.GenGuidString(),
                                LastestUpdatedTime = DateTime.Now,
                                Name = component.Name,
                                Image = "",
                                ProductStageId = null,
                                ProductComponentMaterials = new List<ProductComponentMaterial>()
                                    {
                                     new ProductComponentMaterial()
                                     {
                                         Id = Ultils.GenGuidString(),
                                         MaterialId = materialId
                                     }
                                    }
                            });
                        }
                        //}));
                    }
                }
                else
                {
                    foreach (var type in templateComponentTypes)
                    {
                        //tasksSetComponents.Add(Task.Run(() =>
                        //{
                        var component = templateComponents.SingleOrDefault(x => x.ComponentTypeId == type.Id && x.Default == true);

                        if (component != null)
                        {
                            saveOrderComponents.Add(new ProductComponent()
                            {
                                ComponentId = component.Id,
                                Id = Ultils.GenGuidString(),
                                LastestUpdatedTime = DateTime.Now,
                                Name = component.Name,
                                Image = "",
                                ProductStageId = null,
                                ProductComponentMaterials = new List<ProductComponentMaterial>()
                                    {
                                     new ProductComponentMaterial()
                                     {
                                         Id = Ultils.GenGuidString(),
                                         MaterialId = materialId
                                     }
                                    }
                            });
                        }
                        //}));
                    }
                }

                await Task.WhenAll(tasksSetComponents);

                product.SaveOrderComponents = JsonConvert.SerializeObject(saveOrderComponents);
                //}));
                #endregion

                await Task.WhenAll(tasks);

                if (productRepository.Create(product))
                {
                    return await productBodySizeService.UpdateProductBodySize(product.Id, template.Id, profileId, order.CustomerId) ? product.Id : null;
                }
                else
                {
                    throw new SystemsException("Lỗi trong quá trình tạo sản phẩm");
                }
                return null;
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<string> UpdateProduct(string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                if (dbOrder.Status >= 2)
                {
                    throw new UserException("Đơn hàng đã duyệt. Không thể chỉnh sửa");
                }
                else
                {
                    var dbProduct = productRepository.Get(product.Id);
                    if (dbProduct != null && dbProduct.IsActive == true)
                    {
                        if (dbProduct.Status >= 2)
                        {
                            throw new UserException("Sản phẩm trong giai đoạn thực hiện. Không thể chỉnh sửa");
                        }
                        else
                        {
                            if (product.ProductTemplateId != dbProduct.ProductTemplateId)
                            {
                                throw new UserException("Không thể thay đổi bản mẫu của sản phẩm");
                            }
                            else
                            {
                                var tasks = new List<Task>();
                                //bản mẫu
                                var template = productTemplateRepository.Get(product.ProductTemplateId);

                                // các bộ phận của bản mẫu
                                var templateComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true).ToList();

                                // các kiểu bộ phận của bản mẫu
                                var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == template.Id && x.IsActive == true).ToList();

                                var material = materialRepository.Get(materialId);

                                var materialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);

                                tasks.Add(Task.Run(() =>
                                {
                                    if (!string.IsNullOrWhiteSpace(product.Name))
                                    {
                                        dbProduct.Name = product.Name;
                                    }
                                }));

                                tasks.Add(Task.Run(() =>
                                {
                                    if (!string.IsNullOrWhiteSpace(product.Note))
                                    {
                                        dbProduct.Note = product.Note;
                                    }
                                }));

                                tasks.Add(Task.Run(() =>
                                {
                                    dbProduct.LastestUpdatedTime = DateTime.Now;
                                }));

                                tasks.Add(Task.Run(() =>
                                {
                                    if (isCusMaterial)
                                    {
                                        dbProduct.Price = template.Price;
                                    }
                                    else
                                    {
                                        dbProduct.Price = template.Price + Math.Abs(Math.Round((decimal)((double)materialCategory.PricePerUnit * materialQuantity), 2));
                                    }
                                }));

                                tasks.Add(Task.Run(async () =>
                                {
                                    var tasksSetComponents = new List<Task>();

                                    var saveOrderComponents = new List<ProductComponent>();

                                    if (templateComponentTypes.Any())
                                    {
                                        foreach (var type in templateComponentTypes)
                                        {
                                            tasksSetComponents.Add(Task.Run(() =>
                                            {
                                                var productComponentAdds = templateComponents.Where(x => x.ComponentTypeId == type.Id && productComponents.Select(c => c.ComponentId).Contains(x.Id));
                                                var component = new Component();
                                                if (productComponentAdds != null && productComponentAdds.Any())
                                                {
                                                    if (productComponentAdds.Count() > 1)
                                                    {
                                                        throw new UserException("Chỉ được chọn 1 kiểu cho 1 bộ phận");
                                                    }
                                                    else
                                                    {
                                                        component = productComponentAdds.First();
                                                    }
                                                }
                                                else
                                                {
                                                    component = templateComponents.SingleOrDefault(x => x.ComponentTypeId == type.Id && x.Default == true);
                                                }

                                                if (component != null)
                                                {
                                                    saveOrderComponents.Add(new ProductComponent()
                                                    {
                                                        ComponentId = component.Id,
                                                        Id = Ultils.GenGuidString(),
                                                        LastestUpdatedTime = DateTime.Now,
                                                        Name = component.Name,
                                                        Image = "",
                                                        ProductStageId = null,
                                                        ProductComponentMaterials = new List<ProductComponentMaterial>()
                                                    {
                                                     new ProductComponentMaterial()
                                                     {
                                                         Id = Ultils.GenGuidString(),
                                                         MaterialId = materialId
                                                     }
                                                    }
                                                    });
                                                }
                                            }));
                                        }
                                    }
                                    else
                                    {
                                        foreach (var type in templateComponentTypes)
                                        {
                                            tasksSetComponents.Add(Task.Run(() =>
                                            {
                                                var component = templateComponents.SingleOrDefault(x => x.ComponentTypeId == type.Id && x.Default == true);

                                                if (component != null)
                                                {
                                                    saveOrderComponents.Add(new ProductComponent()
                                                    {
                                                        ComponentId = component.Id,
                                                        Id = Ultils.GenGuidString(),
                                                        LastestUpdatedTime = DateTime.Now,
                                                        Name = component.Name,
                                                        Image = "",
                                                        ProductStageId = null,
                                                        ProductComponentMaterials = new List<ProductComponentMaterial>()
                                                    {
                                                     new ProductComponentMaterial()
                                                     {
                                                         Id = Ultils.GenGuidString(),
                                                         MaterialId = materialId
                                                     }
                                                    }
                                                    });
                                                }
                                            }));
                                        }
                                    }

                                    await Task.WhenAll(tasksSetComponents);

                                    dbProduct.SaveOrderComponents = JsonConvert.SerializeObject(saveOrderComponents);
                                }));

                                if (productRepository.Update(dbProduct.Id, dbProduct))
                                {
                                    return await productBodySizeService.UpdateProductBodySize(product.Id, template.Id, profileId, dbOrder.CustomerId) ? dbProduct.Id : null;
                                }
                                else
                                {
                                    throw new SystemsException("Lỗi trong quá trình cập nhật sản phẩm");
                                }
                                return null;
                            }
                        }
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy sản phẩm");
                    }
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
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

        public IEnumerable<Product> GetProductsByOrderId(string? orderId)
        {
            return productRepository.GetAll(x => ((orderId != null && x.OrderId.Trim().ToLower() == orderId.ToLower().Trim())) && x.IsActive == true);
        }

        public IEnumerable<Product> GetProductsByOrderIdOfCus(string? orderId, string cusId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.CustomerId == cusId && dbOrder.IsActive == true && dbOrder.Status >= 1)
            {
                return productRepository.GetAll(x => ((orderId != null && x.OrderId.Trim().ToLower() == orderId.ToLower().Trim())) && x.IsActive == true);
            }
            else
            {
                return null;
            }
        }
    }
}
