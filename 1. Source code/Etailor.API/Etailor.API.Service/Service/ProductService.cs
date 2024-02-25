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

        public ProductService(IProductRepository productRepository, IProductTemplateRepository productTemplateRepository,
            IOrderRepository orderRepository, ITemplateStateRepository templateStateRepository, IComponentTypeRepository componentTypeRepository,
            IComponentRepository componentRepository, IComponentStageRepository componentStageRepository, IProductStageRepository productStageRepository,
            IProductComponentRepository productComponentRepository, IMaterialRepository materialRepository, IProfileBodyRepository profileBodyRepository,
            IProductBodySizeService productBodySizeService)
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
        }

        public async Task<bool> AddProduct(string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId)
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

                // các bước mẫu
                var templateStages = templateStateRepository.GetAll(x => x.ProductTemplateId == product.ProductTemplateId && x.IsActive == true).ToList();

                // các bộ phận được xử lý trong bước mẫu
                var componentStages = componentStageRepository.GetAll(x => templateStages.Select(t => t.Id).Contains(x.TemplateStageId)).ToList();

                // các bộ phận của bản mẫu
                var templateComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true).ToList();

                // các kiểu bộ phận của bản mẫu
                var templateComponents = componentRepository.GetAll(x => x.ProductTemplateId == template.Id && x.IsActive == true).ToList();

                // các kiểu bộ phận của sản phẩm
                var componentOfProducts = componentRepository.GetAll(x => productComponents.Select(c => c.ComponentId).Contains(x.Id)).ToList();

                var storeMaterialIds = materialRepository.GetAll(x => x.Quantity > 0 && x.IsActive == true).Select(x => x.Id).ToList();

                // khởi tạo các bước của sản phẩm
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

                // kiểm tra nếu tạo sản phẩm thành công
                if (productRepository.Create(product))
                {
                    await productBodySizeService.CreateProductBodySize(product.Id, template.Id, profileId, order.CustomerId);

                    // kiểm tra nếu các bước của sản phẩm có
                    if (productStages.Any())
                    {
                        // kiểm tra nếu tạo các bước của sản phẩm thành công
                        if (productStageRepository.CreateRange(productStages))
                        {
                            // kiểm tra nếu các kiểu bộ phận của sản phẩm không khớp với các kiểu bộ phận của bản mẫu
                            if (componentOfProducts.Where(x => !templateComponents.Select(c => c.Id).ToList().Contains(x.Id)).Any())
                            {
                                throw new SystemsException("Kiểu bộ phận không phù hợp");
                            }
                            else
                            {
                                // khởi tạo list product component để add db
                                var productCompnentCreates = new List<ProductComponent>();

                                // duyệt từng bước của bản mẫu
                                foreach (var templateStage in templateStages)
                                {
                                    tasks.Add(Task.Run(async () =>
                                    {
                                        var createProductComponentTasks = new List<Task>();
                                        var componentTypesInTemplateStageIds = new List<string>();
                                        var componentTypesInTemplateStages = new List<ComponentType>();
                                        var types = new List<Component>();

                                        createProductComponentTasks.Add(Task.Run(() =>
                                        {
                                            // Lấy ds id các component type của bước mẫu
                                            componentTypesInTemplateStageIds = componentStages.Where(x => x.TemplateStageId == templateStage.Id).Select(x => x.Id).ToList();
                                        }));

                                        createProductComponentTasks.Add(Task.Run(() =>
                                        {
                                            // Lấy ds các component của bước mẫu
                                            componentTypesInTemplateStages = templateComponentTypes.Where(x => componentTypesInTemplateStageIds.Contains(x.Id)).ToList();
                                        }));

                                        createProductComponentTasks.Add(Task.Run(() =>
                                        {
                                            // lấy ra các kiểu bộ phận của sản phẩm tương ứng với bộ phận đó
                                            var types = componentOfProducts.Where(x => componentTypesInTemplateStageIds.Contains(x.ComponentTypeId)).ToList();
                                        }));

                                        await Task.WhenAll(createProductComponentTasks);

                                        // nếu có kiểu bộ phận tương ứng
                                        if (types.Any())
                                        {
                                            // nếu có nhiều hơn 1 kiểu bộ phận
                                            if (types.Count > 1)
                                            {
                                                throw new SystemsException("Mỗi bộ phận chỉ có thể chọn 1 kiểu");
                                            }
                                            else
                                            {
                                                // lấy phần tử đầu tiên
                                                var type = types.First();
                                                var productStage = new ProductStage();
                                                var productComponent = new ProductComponent();

                                                createProductComponentTasks.Add(Task.Run(() =>
                                                {
                                                    productStage = productStages.Where(x => x.StageNum == templateStage.StageNum).First();
                                                }));
                                                createProductComponentTasks.Add(Task.Run(() =>
                                                {
                                                    productComponent = productComponents.Where(x => x.ComponentId == type.Id).FirstOrDefault();
                                                }));

                                                await Task.WhenAll(createProductComponentTasks);

                                                if (productComponent != null)
                                                {
                                                    productComponent.Id = Ultils.GenGuidString();
                                                    createProductComponentTasks.Add(Task.Run(() =>
                                                    {
                                                        productComponent.Name = type.Name;
                                                    }));
                                                    createProductComponentTasks.Add(Task.Run(() =>
                                                    {
                                                        productComponent.ComponentId = type.Id;
                                                    }));
                                                    createProductComponentTasks.Add(Task.Run(() =>
                                                    {
                                                        productComponent.ProductStageId = productStage.Id;
                                                    }));
                                                    createProductComponentTasks.Add(Task.Run(() =>
                                                    {
                                                        productComponent.LastestUpdatedTime = DateTime.Now;
                                                    }));

                                                    await Task.WhenAll(createProductComponentTasks);

                                                    if (!string.IsNullOrWhiteSpace(materialId) && storeMaterialIds.Contains(materialId))
                                                    {
                                                        productComponent.ProductComponentMaterials = new List<ProductComponentMaterial>()
                                                        {
                                                            new ProductComponentMaterial()
                                                                {
                                                                    Id = Ultils.GenGuidString(),
                                                                    MaterialId = materialId,
                                                                    ProductComponentId = productComponent.Id,
                                                                    Quantity = 0
                                                                }
                                                        };
                                                    }
                                                    else
                                                    {
                                                        throw new UserException("Nguyên liệu không tồn tại trong hệ thống");
                                                    }

                                                    productCompnentCreates.Add(productComponent);
                                                }
                                            }
                                        }
                                    }));
                                }

                                await Task.WhenAll(tasks);

                                return productComponentRepository.CreateRange(productCompnentCreates);
                            }
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

        public async Task<bool> UpdateProduct(string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.IsActive == true)
            {
                if (dbOrder.Status >= 5)
                {
                    throw new UserException("Đơn hàng đã vào giai đoạn hoàn thiện. Không thể chỉnh sửa");
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
                                if (!string.IsNullOrWhiteSpace(product.Name))
                                {
                                    dbProduct.Name = product.Name;
                                }
                                if (!string.IsNullOrWhiteSpace(product.Note))
                                {
                                    dbProduct.Note = product.Note;
                                }

                                var dbProductStageIds = productStageRepository.GetAll(x => x.ProductId == dbProduct.Id && x.IsActive == true).Select(x => x.Id).ToList();

                                var dbProductComponents = productComponentRepository.GetAll(x => dbProductStageIds.Contains(x.Id)).ToList();

                                var dbProductComponentIds = dbProductComponents.Select(x => x.ComponentId).ToList();






                                return false;
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

        public IEnumerable<Product> GetProductsByOrderId(string? search)
        {
            return productRepository.GetAll(x => ((search != null && x.OrderId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
