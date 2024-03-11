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
using Microsoft.AspNetCore.Components;
using Etailor.API.Repository.Repository;

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
        private readonly IOrderService orderService;
        private readonly IStaffRepository staffRepository;
        private readonly IMasteryRepository masteryRepository;

        public ProductService(IProductRepository productRepository, IProductTemplateRepository productTemplateRepository,
            IOrderRepository orderRepository, ITemplateStateRepository templateStateRepository, IComponentTypeRepository componentTypeRepository,
            IComponentRepository componentRepository, IComponentStageRepository componentStageRepository, IProductStageRepository productStageRepository,
            IProductComponentRepository productComponentRepository, IMaterialRepository materialRepository, IProfileBodyRepository profileBodyRepository,
            IProductBodySizeService productBodySizeService, IMaterialTypeRepository materialTypeRepository, IMaterialCategoryRepository materialCategoryRepository,
            IOrderMaterialRepository orderMaterialRepository, IOrderService orderService, IStaffRepository staffRepository, IMasteryRepository masteryRepository)
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
            this.orderService = orderService;
            this.staffRepository = staffRepository;
            this.masteryRepository = masteryRepository;
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

                //các bộ phận của bản mẫu
                var templateComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true).ToList();

                //các kiểu bộ phận của bản mẫu
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
                    else
                    {
                        product.FabricMaterialId = materialId;
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
                                        //   ProductComponentMaterials = new List<ProductComponentMaterial>() //cái này chờ kéo materialId vào product
                                        //   {
                                        //new ProductComponentMaterial()
                                        //{
                                        //    Id = Ultils.GenGuidString(),
                                        //    MaterialId = materialId,
                                        //}
                                        //   }
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
                                        //   ProductComponentMaterials = new List<ProductComponentMaterial>()
                                        //   {
                                        //new ProductComponentMaterial()
                                        //{
                                        //    Id = Ultils.GenGuidString(),
                                        //    MaterialId = materialId
                                        //}
                                        //   }
                                    });
                                }
                            }));
                        }
                    }

                    await Task.WhenAll(tasksSetComponents);

                    product.SaveOrderComponents = JsonConvert.SerializeObject(saveOrderComponents);
                }));
                #endregion

                await Task.WhenAll(tasks);

                if (productRepository.Create(product))
                {
                    if (await orderService.CheckOrderPaid(product.OrderId))
                    {
                        try
                        {
                            if (await productBodySizeService.UpdateProductBodySize(product.Id, template.Id, profileId, order.CustomerId))
                            {
                                product.ReferenceProfileBodyId = profileId;

                                return productRepository.Update(product.Id, product) ? product.Id : null;
                            }
                            else
                            {
                                throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo số đo sản phẩm",nameof(ProductService));
                            }
                        }
                        catch (UserException uex)
                        {
                            product.IsActive = false;
                            product.InactiveTime = DateTime.Now;

                            if (productRepository.Update(product.Id, product))
                            {
                                throw new UserException(uex.Message);
                            }
                        }
                    }
                    else
                    {
                        throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật hóa đơn", nameof(ProductService));
                    }
                }
                else
                {
                    throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo sản phẩm", nameof(ProductService));
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
            if (dbOrder != null)
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
                                                        //    ProductComponentMaterials = new List<ProductComponentMaterial>()
                                                        //{
                                                        // new ProductComponentMaterial()
                                                        // {
                                                        //     Id = Ultils.GenGuidString(),
                                                        //     MaterialId = materialId
                                                        // }
                                                        //}
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
                                                        //    ProductComponentMaterials = new List<ProductComponentMaterial>()
                                                        //{
                                                        // new ProductComponentMaterial()
                                                        // {
                                                        //     Id = Ultils.GenGuidString(),
                                                        //     MaterialId = materialId
                                                        // }
                                                        //}
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
                                    if (await orderService.CheckOrderPaid(dbProduct.OrderId))
                                    {
                                        if (await productBodySizeService.UpdateProductBodySize(product.Id, template.Id, profileId, dbOrder.CustomerId))
                                        {
                                            product.ReferenceProfileBodyId = profileId;

                                            return productRepository.Update(dbProduct.Id, dbProduct) ? dbProduct.Id : null;
                                        }
                                        else
                                        {
                                            throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo số đo sản phẩm", nameof(ProductService));
                                        }
                                    }
                                    else
                                    {
                                        throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật hóa đơn", nameof(ProductService));
                                    }
                                }
                                else
                                {
                                    throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật sản phẩm", nameof(ProductService));
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

                if (productRepository.Update(dbProduct.Id, dbProduct))
                {
                    if (await orderService.CheckOrderPaid(dbProduct.OrderId))
                    {
                        return true;
                    }
                    else
                    {
                        throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật hóa đơn", nameof(ProductService));
                    }
                }
                else
                {
                    throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật sản phẩm", nameof(ProductService));
                }
            }
            else
            {
                throw new UserException("Không tìm thấy sản phầm");
            }
        }

        public async Task<Product> GetProductOrder(string id, string orderId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.Status >= 1)
            {
                var product = productRepository.Get(id);
                if (product != null && product.Status >= 1 && product.IsActive == true)
                {
                    if (!string.IsNullOrEmpty(product.ProductTemplateId))
                    {
                        product.ProductTemplate = productTemplateRepository.Get(product.ProductTemplateId);
                    }
                    if (!string.IsNullOrEmpty(product.FabricMaterialId))
                    {
                        product.FabricMaterial = materialRepository.Get(product.FabricMaterialId);
                    }
                    if (product.ProductTemplate != null)
                    {
                        product.ProductTemplate.ThumbnailImage = await Ultils.GetUrlImage(product.ProductTemplate.ThumbnailImage);
                    }
                    return product;
                }
            }
            return null;
        }

        public async Task<Product> GetProductOrderByCus(string id, string orderId, string cusId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.IsActive == true && dbOrder.Status >= 1 && dbOrder.CustomerId == cusId)
            {
                var product = productRepository.Get(id);
                if (product != null && product.IsActive == true)
                {
                    if (!string.IsNullOrEmpty(product.ProductTemplateId))
                    {
                        product.ProductTemplate = productTemplateRepository.Get(product.ProductTemplateId);
                    }
                    if (!string.IsNullOrEmpty(product.FabricMaterialId))
                    {
                        product.FabricMaterial = materialRepository.Get(product.FabricMaterialId);
                    }
                    await Task.WhenAll(Task.Run(async () =>
                    {
                        if (product.ProductTemplate != null && !string.IsNullOrEmpty(product.ProductTemplate.ThumbnailImage))
                        {
                            product.ProductTemplate.ThumbnailImage = await Ultils.GetUrlImage(product.ProductTemplate.ThumbnailImage);
                        }
                    }),
                    Task.Run(async () =>
                    {
                        if (product.FabricMaterial != null && !string.IsNullOrEmpty(product.FabricMaterial.Image))
                        {
                            product.FabricMaterial.Image = await Ultils.GetUrlImage(product.FabricMaterial.Image);
                        }
                    }));
                    return product;
                }
            }
            return null;
        }

        public async Task<IEnumerable<Product>> GetProductsByOrderId(string orderId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.Status >= 1)
            {
                var products = productRepository.GetAll(x => x.OrderId == orderId && x.IsActive == true).ToList();

                if (products.Any() && products.Count() > 0)
                {
                    products = products.ToList();

                    var templates = productTemplateRepository.GetAll(x => products.Select(p => p.ProductTemplateId).Contains(x.Id));
                    if (templates != null && templates.Any())
                    {
                        templates = templates.ToList();

                        var fabricMaterials = materialRepository.GetAll(x => products.Select(p => p.FabricMaterialId).Contains(x.Id));

                        if (fabricMaterials != null && fabricMaterials.Any())
                        {
                            fabricMaterials = fabricMaterials.ToList();

                            var tasks = new List<Task>();

                            foreach (var product in products)
                            {
                                tasks.Add(Task.Run(async () =>
                                {
                                    var setValueTasks = new List<Task>();
                                    setValueTasks.Add(Task.Run(async () =>
                                    {
                                        product.ProductTemplate = templates.SingleOrDefault(x => x.Id == product.ProductTemplateId);
                                        if (product.ProductTemplate != null && !string.IsNullOrEmpty(product.ProductTemplate.ThumbnailImage))
                                        {
                                            product.ProductTemplate.ThumbnailImage = await Ultils.GetUrlImage(product.ProductTemplate.ThumbnailImage);
                                        }
                                    }));
                                    setValueTasks.Add(Task.Run(async () =>
                                    {
                                        product.FabricMaterial = fabricMaterials.SingleOrDefault(x => x.Id == product.FabricMaterialId);
                                        if (product.FabricMaterial != null && !string.IsNullOrEmpty(product.FabricMaterial.Image))
                                        {
                                            product.FabricMaterial.Image = await Ultils.GetUrlImage(product.FabricMaterial.Image);
                                        }
                                    }));

                                    await Task.WhenAll(setValueTasks);
                                }));
                            }

                            await Task.WhenAll(tasks);

                            return products;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Product>> GetProductsByOrderIds(List<string> orderIds)
        {
            var dbOrders = orderRepository.GetAll(x => orderIds.Contains(x.Id) && x.Status >= 1);
            if (dbOrders != null && dbOrders.Any())
            {
                dbOrders = dbOrders.ToList();
                var products = productRepository.GetAll(x => dbOrders.Select(o => o.Id).Contains(x.OrderId) && x.IsActive == true);

                if (products.Any() && products.Count() > 0)
                {
                    products = products.ToList();

                    var templates = productTemplateRepository.GetAll(x => products.Select(p => p.ProductTemplateId).Contains(x.Id));
                    if (templates != null && templates.Any())
                    {
                        templates = templates.ToList();

                        var fabricMaterials = materialRepository.GetAll(x => products.Select(p => p.FabricMaterialId).Contains(x.Id));

                        if (fabricMaterials != null && fabricMaterials.Any())
                        {
                            fabricMaterials = fabricMaterials.ToList();

                            var tasks = new List<Task>();

                            foreach (var product in products)
                            {
                                tasks.Add(Task.Run(async () =>
                                {
                                    var setValueTasks = new List<Task>();
                                    setValueTasks.Add(Task.Run(async () =>
                                    {
                                        product.ProductTemplate = templates.SingleOrDefault(x => x.Id == product.ProductTemplateId);
                                        if (product.ProductTemplate != null && !string.IsNullOrEmpty(product.ProductTemplate.ThumbnailImage))
                                        {
                                            product.ProductTemplate.ThumbnailImage = await Ultils.GetUrlImage(product.ProductTemplate.ThumbnailImage);
                                        }
                                    }));
                                    setValueTasks.Add(Task.Run(async () =>
                                    {
                                        product.FabricMaterial = fabricMaterials.SingleOrDefault(x => x.Id == product.FabricMaterialId);
                                        if (product.FabricMaterial != null && !string.IsNullOrEmpty(product.FabricMaterial.Image))
                                        {
                                            product.FabricMaterial.Image = await Ultils.GetUrlImage(product.FabricMaterial.Image);
                                        }
                                    }));

                                    await Task.WhenAll(setValueTasks);
                                }));
                            }

                            await Task.WhenAll(tasks);

                            return products;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Product>> GetProductsByOrderIdOfCus(string orderId, string cusId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.CustomerId == cusId && dbOrder.IsActive == true && dbOrder.Status >= 1)
            {
                var products = productRepository.GetAll(x => x.OrderId == orderId && x.IsActive == true).ToList();

                if (products.Any() && products.Count > 0)
                {
                    var templates = productTemplateRepository.GetAll(x => products.Select(p => p.ProductTemplateId).Contains(x.Id)).ToList();
                    var fabricMaterials = materialRepository.GetAll(x => products.Select(p => p.FabricMaterialId).Contains(x.Id)).ToList();

                    var tasks = new List<Task>();

                    foreach (var product in products)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            var setValueTasks = new List<Task>();
                            setValueTasks.Add(Task.Run(async () =>
                            {
                                product.ProductTemplate = templates.SingleOrDefault(x => x.Id == product.ProductTemplateId);
                                if (product.ProductTemplate != null && !string.IsNullOrEmpty(product.ProductTemplate.ThumbnailImage))
                                {
                                    product.ProductTemplate.ThumbnailImage = await Ultils.GetUrlImage(product.ProductTemplate.ThumbnailImage);
                                }
                            }));
                            setValueTasks.Add(Task.Run(async () =>
                            {
                                product.FabricMaterial = fabricMaterials.SingleOrDefault(x => x.Id == product.FabricMaterialId);
                                if (product.FabricMaterial != null && !string.IsNullOrEmpty(product.FabricMaterial.Image))
                                {
                                    product.FabricMaterial.Image = await Ultils.GetUrlImage(product.FabricMaterial.Image);
                                }
                            }));

                            await Task.WhenAll(setValueTasks);
                        }));
                    }

                    await Task.WhenAll(tasks);

                    return products;
                }
            }
            return null;
        }

        public void AutoCreateEmptyTaskProduct()
        {
            try
            {
                Ultils.SendMessageToDev("Run func AutoCreateEmptyTaskProduct");
                var approveOrders = orderRepository.GetAll(x => x.Status == 2 && x.IsActive == true);
                if (approveOrders != null && approveOrders.Any())
                {
                    approveOrders = approveOrders.OrderBy(x => x.CreatedTime).ToList();

                    var approveOrdersProducts = productRepository.GetAll(x => approveOrders.Select(o => o.Id).Contains(x.OrderId) && x.Status == 1 && x.IsActive == true);
                    if (approveOrdersProducts != null && approveOrdersProducts.Any())
                    {
                        approveOrdersProducts = approveOrdersProducts.ToList();

                        var templates = productTemplateRepository.GetAll(x => approveOrdersProducts.Select(c => c.ProductTemplateId).Contains(x.Id));
                        if (templates != null && templates.Any())
                        {
                            templates = templates.ToList();

                            var templateStages = templateStateRepository.GetAll(x => templates.Select(c => c.Id).Contains(x.ProductTemplateId) && x.IsActive == true);
                            if (templateStages != null && templateStages.Any())
                            {
                                templateStages = templateStages.ToList();

                                var stageComponents = componentStageRepository.GetAll(x => templateStages.Select(c => c.Id).Contains(x.TemplateStageId));
                                if (stageComponents != null && stageComponents.Any())
                                {
                                    stageComponents = stageComponents.ToList();

                                    var components = componentRepository.GetAll(x => templates.Select(c => c.Id).Contains(x.ProductTemplateId) && x.IsActive == true);
                                    if (components != null && components.Any())
                                    {
                                        components = components.ToList();

                                        var productAllDbs = productRepository.GetAll(x => x.IsActive == true && x.Status > 0);
                                        int? greatestIndexDb = 0;

                                        if (productAllDbs != null && productAllDbs.Any())
                                        {
                                            greatestIndexDb = productAllDbs.OrderByDescending(x => x.Index).FirstOrDefault()?.Index;
                                        }
                                        if (greatestIndexDb == null)
                                        {
                                            greatestIndexDb = 1;
                                        }
                                        else
                                        {
                                            greatestIndexDb++;
                                        }

                                        var check = new List<bool>();

                                        foreach (var order in approveOrders)
                                        {
                                            if (approveOrdersProducts.Any(x => x.OrderId == order.Id))
                                            {
                                                order.Products = approveOrdersProducts.Where(x => x.OrderId == order.Id).ToList();
                                                foreach (var product in order.Products)
                                                {
                                                    product.Index = greatestIndexDb;

                                                    if (!string.IsNullOrWhiteSpace(product.SaveOrderComponents))
                                                    {
                                                        var productTemplate = templates.FirstOrDefault(x => x.Id == product.ProductTemplateId);
                                                        if (productTemplate != null)
                                                        {
                                                            var productTemplateStagees = templateStages.Where(x => x.ProductTemplateId == product.ProductTemplateId);
                                                            if (productTemplateStagees != null && productTemplateStagees.Any())
                                                            {
                                                                productTemplateStagees = productTemplateStagees.ToList();

                                                                product.ProductStages = new List<ProductStage>();

                                                                foreach (var stage in productTemplateStagees.OrderBy(x => x.StageNum))
                                                                {
                                                                    var productStage = new ProductStage()
                                                                    {
                                                                        Id = Ultils.GenGuidString(),
                                                                        Deadline = null,
                                                                        StartTime = null,
                                                                        FinishTime = null,
                                                                        InactiveTime = null,
                                                                        IsActive = true,
                                                                        ProductId = product.Id,
                                                                        StaffId = null,
                                                                        StageNum = stage.StageNum,
                                                                        TaskIndex = null,
                                                                        Status = 1,
                                                                        TemplateStageId = stage.Id,
                                                                        ProductComponents = new List<ProductComponent>()
                                                                    };

                                                                    var componentTypesInStage = stageComponents.Where(x => x.TemplateStageId == stage.Id);
                                                                    if (componentTypesInStage != null && componentTypesInStage.Any())
                                                                    {
                                                                        componentTypesInStage = componentTypesInStage.ToList();

                                                                        var componentsInStage = components.Where(x => componentTypesInStage.Select(c => c.ComponentTypeId).Contains(x.ComponentTypeId));
                                                                        if (componentsInStage != null && componentsInStage.Any())
                                                                        {
                                                                            componentsInStage = componentsInStage.ToList();

                                                                            var productComponents = JsonConvert.DeserializeObject<List<ProductComponent>>(product.SaveOrderComponents);
                                                                            if (productComponents != null && productComponents.Any())
                                                                            {
                                                                                var productComponent = productComponents.FirstOrDefault(x => componentsInStage.Select(c => c.Id).Contains(x.ComponentId));
                                                                                if (productComponent != null)
                                                                                {
                                                                                    productComponent.LastestUpdatedTime = DateTime.Now;
                                                                                    productComponent.Name = components.FirstOrDefault(x => x.Id == productComponent.ComponentId)?.Name;
                                                                                    productComponent.ProductStageId = productStage.Id;
                                                                                    productComponent.ProductComponentMaterials = null;

                                                                                    productStage.ProductComponents.Add(productComponent);
                                                                                }
                                                                            }
                                                                        }
                                                                    }

                                                                    product.ProductStages.Add(productStage);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    greatestIndexDb++;
                                                }
                                            }
                                            order.Status = 3;
                                            check.Add(orderRepository.Update(order.Id, order));
                                        }

                                        if (check.Any(x => x == false))
                                        {
                                            throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình tự động tạo task", nameof(ProductService));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                AutoAssignTaskForStaff();
            }
            catch (Exception ex)
            {
                throw new SystemsException($"Error in {nameof(ProductService)}: {ex.Message}", nameof(ProductService));
            }
        }

        public void AutoAssignTaskForStaff()
        {
            try
            {
                var checkException = false;
                Ultils.SendMessageToDev("Run func AutoCreateEmptyTaskProduct");
                var notStartOrders = orderRepository.GetAll(x => x.Status == 3 && x.IsActive == true);
                if (notStartOrders != null && notStartOrders.Any())
                {
                    notStartOrders = notStartOrders.OrderBy(x => x.CreatedTime).ToList();

                    var notStartEmptyTaskProducts = productRepository.GetAll(x => notStartOrders.Select(c => c.Id).Contains(x.OrderId) && x.StaffMakerId == null && x.Status > 0 && x.IsActive == true);
                    if (notStartEmptyTaskProducts != null && notStartEmptyTaskProducts.Any())
                    {
                        notStartEmptyTaskProducts = notStartEmptyTaskProducts.ToList();

                        var templates = productTemplateRepository.GetAll(x => notStartEmptyTaskProducts.Select(c => c.ProductTemplateId).Contains(x.Id));
                        if (templates != null && templates.Any())
                        {
                            templates = templates.ToList();

                            var allMasteryStaffs = masteryRepository.GetAll(x => templates.Select(c => c.CategoryId).Contains(x.CategoryId));
                            if (allMasteryStaffs != null && allMasteryStaffs.Any())
                            {
                                allMasteryStaffs = allMasteryStaffs.ToList();

                                var allStaffs = staffRepository.GetAll(x => allMasteryStaffs.Select(c => c.StaffId).Contains(x.Id) && (x.Role == 1 || x.Role == 2) && x.IsActive == true);
                                if (allStaffs != null && allStaffs.Any())
                                {
                                    allStaffs = allStaffs.ToList();

                                    foreach (var product in notStartEmptyTaskProducts)
                                    {
                                        product.ProductTemplate = templates.SingleOrDefault(x => x.Id == product.ProductTemplateId);
                                        if (product.ProductTemplate != null)
                                        {
                                            var masteryStaffs = allMasteryStaffs.Where(x => x.CategoryId == product.ProductTemplate.CategoryId);
                                            if (masteryStaffs != null && masteryStaffs.Any())
                                            {
                                                masteryStaffs = masteryStaffs.ToList();

                                                var staffs = allStaffs.Where(x => masteryStaffs.Select(c => c.StaffId).Contains(x.Id) && x.IsActive == true);
                                                if (staffs != null && staffs.Any())
                                                {
                                                    staffs = staffs.ToList();

                                                    var findFreeStaff = new Dictionary<string, string>();
                                                    findFreeStaff.Add("Id", "");
                                                    findFreeStaff.Add("NumOfTask", "-1");
                                                    findFreeStaff.Add("MaxIndex", "0");

                                                    foreach (var staff in staffs)
                                                    {
                                                        var staffCurrentTasks = productRepository.GetAll(x => x.StaffMakerId == staff.Id && x.Status > 0 && x.Status < 4 && x.IsActive == true);
                                                        if (staffCurrentTasks != null && staffCurrentTasks.Any())
                                                        {
                                                            staffCurrentTasks = staffCurrentTasks.OrderByDescending(x => x.Index).ToList();

                                                            var numOfTasks = staffCurrentTasks.Count();

                                                            if (findFreeStaff.Any())
                                                            {
                                                                findFreeStaff.TryGetValue("NumOfTask", out string numOfTask1);
                                                                int.TryParse(numOfTask1, out int numOfTaskInt);
                                                                if (numOfTaskInt == -1 || numOfTaskInt > numOfTasks)
                                                                {
                                                                    findFreeStaff.Clear();
                                                                    findFreeStaff.Add("Id", staff.Id);
                                                                    findFreeStaff.Add("NumOfTask", numOfTasks + "");
                                                                    findFreeStaff.Add("MaxIndex", staffCurrentTasks.First().Index + "");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                findFreeStaff.Clear();
                                                                findFreeStaff.Add("Id", staff.Id);
                                                                findFreeStaff.Add("NumOfTask", numOfTasks + "");
                                                                findFreeStaff.Add("MaxIndex", staffCurrentTasks.First().Index + "");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            findFreeStaff.Clear();
                                                            findFreeStaff.Add("Id", staff.Id);
                                                            findFreeStaff.Add("NumOfTask", "0");
                                                            findFreeStaff.Add("MaxIndex", "0");
                                                        }
                                                    }
                                                    findFreeStaff.TryGetValue("Id", out string staffId);
                                                    product.StaffMakerId = staffId;

                                                    findFreeStaff.TryGetValue("MaxIndex", out string maxIndex);
                                                    int.TryParse(maxIndex, out int maxIndexInt);
                                                    product.Index = maxIndexInt + 1;
                                                    try
                                                    {
                                                        if (!productRepository.Update(product.Id, product))
                                                        {
                                                            throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình tự động phân công công việc cho nhân viên", nameof(ProductService));
                                                        }
                                                    }
                                                    catch (SystemsException)
                                                    {
                                                        checkException = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (checkException)
                {
                    throw new SystemsException("Lỗi trong quá trình tự động phân công công việc cho nhân viên", nameof(ProductService));
                }
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(ProductService));
            }
        }

        public Product GetProduct(string id)
        {
            var product = productRepository.Get(id);

            return product == null ? null : product.IsActive == true ? product : null;
        }
    }
}
