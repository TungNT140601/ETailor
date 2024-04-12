using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
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
        private readonly IOrderService orderService;
        private readonly IStaffRepository staffRepository;
        private readonly IMasteryRepository masteryRepository;

        public ProductService(
            IProductRepository productRepository,
            IProductTemplateRepository productTemplateRepository,
            IOrderRepository orderRepository,
            ITemplateStateRepository templateStateRepository,
            IComponentTypeRepository componentTypeRepository,
            IComponentRepository componentRepository,
            IComponentStageRepository componentStageRepository,
            IProductStageRepository productStageRepository,
            IProductComponentRepository productComponentRepository,
            IMaterialRepository materialRepository,
            IProfileBodyRepository profileBodyRepository,
            IProductBodySizeService productBodySizeService,
            IMaterialTypeRepository materialTypeRepository,
            IMaterialCategoryRepository materialCategoryRepository,
            IOrderMaterialRepository orderMaterialRepository,
            IOrderService orderService,
            IStaffRepository staffRepository,
            IMasteryRepository masteryRepository
        )
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

        public async Task<string> AddProduct(
            string wwwroot,
            string orderId,
            Product product,
            List<ProductComponent> productComponents,
            string materialId,
            string profileId,
            bool isCusMaterial,
            double materialQuantity
        )
        {
            var order = orderRepository.Get(orderId);
            if (order != null)
            {
                if (order.Status > 2)
                {
                    throw new UserException(
                        "Đơn hàng đã vào giai đoạn thực hiện. Không thể thêm sản phẩm"
                    );
                }
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
                var templateComponentTypes = componentTypeRepository.GetAll(x =>
                    x.CategoryId == template.CategoryId && x.IsActive == true
                );
                if (templateComponentTypes != null && templateComponentTypes.Any())
                {
                    templateComponentTypes = templateComponentTypes.ToList();
                }

                //các kiểu bộ phận của bản mẫu
                var templateComponents = componentRepository.GetAll(x =>
                    x.ProductTemplateId == template.Id && x.IsActive == true
                );
                if (templateComponents != null && templateComponents.Any())
                {
                    templateComponents = templateComponents.ToList();
                }

                var material = materialRepository.Get(materialId);

                var materialCategory = new MaterialCategory();

                var orderMaterials = orderMaterialRepository.GetAll(x =>
                    x.OrderId == orderId && x.MaterialId == materialId && x.IsActive == true
                );
                if (orderMaterials != null && orderMaterials.Any())
                {
                    orderMaterials = orderMaterials.ToList();
                }
                var addOrderMaterial = new OrderMaterial();
                var updateOrderMaterial = new OrderMaterial();

                if (material != null)
                {
                    materialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);
                }

                #region InitProduct

                tasks.Add(
                    Task.Run(async () =>
                    {
                        if (material == null || material.IsActive == false)
                        {
                            throw new UserException("Không tìm thấy nguyên liệu");
                        }
                        else
                        {
                            product.FabricMaterialId = materialId;
                            var insideTasks = new List<Task>();

                            if (orderMaterials != null && orderMaterials.Any())
                            {
                                orderMaterials = orderMaterials.ToList();
                                if (orderMaterials.Any(x => x.MaterialId == materialId))
                                {
                                    var orderMaterial = orderMaterials.First(x =>
                                        x.MaterialId == materialId
                                    );
                                    insideTasks.Add(
                                        Task.Run(() =>
                                        {
                                            if (orderMaterial.Value != null)
                                            {
                                                orderMaterial.Value += (decimal)materialQuantity;
                                            }
                                            else
                                            {
                                                orderMaterial.Value = (decimal)materialQuantity;
                                            }

                                            updateOrderMaterial = orderMaterial;
                                            addOrderMaterial = null;
                                        })
                                    );
                                }
                                else
                                {
                                    insideTasks.Add(
                                        Task.Run(() =>
                                        {
                                            addOrderMaterial = new OrderMaterial()
                                            {
                                                Id = Ultils.GenGuidString(),
                                                MaterialId = materialId,
                                                OrderId = orderId,
                                                Value = (decimal)materialQuantity,
                                                IsActive = true,
                                                CreatedTime = DateTime.UtcNow.AddHours(7),
                                                LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                                IsCusMaterial = false,
                                                Image = null,
                                                InactiveTime = null
                                            };
                                            updateOrderMaterial = null;
                                        })
                                    );
                                }
                            }
                            else
                            {
                                insideTasks.Add(
                                    Task.Run(() =>
                                    {
                                        addOrderMaterial = new OrderMaterial()
                                        {
                                            Id = Ultils.GenGuidString(),
                                            MaterialId = materialId,
                                            OrderId = orderId,
                                            Value = (decimal)materialQuantity,
                                            IsActive = true,
                                            CreatedTime = DateTime.UtcNow.AddHours(7),
                                            LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                            IsCusMaterial = false,
                                            Image = null,
                                            InactiveTime = null
                                        };
                                        updateOrderMaterial = null;
                                    })
                                );
                            }

                            await Task.WhenAll(insideTasks);
                        }
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.OrderId = orderId;
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        if (string.IsNullOrWhiteSpace(product.Name))
                        {
                            product.Name = template.Name;
                        }
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        if (string.IsNullOrWhiteSpace(product.Note))
                        {
                            product.Note = "";
                        }
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        if (isCusMaterial)
                        {
                            product.Price = template.Price;
                        }
                        else
                        {
                            product.Price =
                                template.Price
                                + Math.Abs(
                                    Math.Round(
                                        (decimal)(
                                            (double)materialCategory.PricePerUnit * materialQuantity
                                        ),
                                        2
                                    )
                                );
                        }
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.Status = 1;
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.EvidenceImage = "";
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.FinishTime = null;
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.CreatedTime = DateTime.UtcNow.AddHours(7);
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.InactiveTime = null;
                    })
                );

                tasks.Add(
                    Task.Run(() =>
                    {
                        product.IsActive = true;
                    })
                );

                tasks.Add(
                    Task.Run(async () =>
                    {
                        var saveOrderComponents = new List<ProductComponent>();

                        if (templateComponentTypes != null && templateComponentTypes.Any())
                        {
                            do
                            {
                                saveOrderComponents = new List<ProductComponent>();
                                var insideTasks = new List<Task>();
                                foreach (var type in templateComponentTypes)
                                {
                                    insideTasks.Add(
                                        Task.Run(async () =>
                                        {
                                            var componentIds = string.Join(
                                                ",",
                                                productComponents.Select(c => c.ComponentId)
                                            );
                                            var productComponentAdds = templateComponents.Where(x =>
                                                x.ComponentTypeId == type.Id
                                                && componentIds.Contains(x.Id)
                                            );
                                            var component = new Component();
                                            if (
                                                productComponentAdds != null
                                                && productComponentAdds.Any()
                                            )
                                            {
                                                if (productComponentAdds.Count() > 1)
                                                {
                                                    throw new UserException(
                                                        "Chỉ được chọn 1 kiểu cho 1 bộ phận"
                                                    );
                                                }
                                                else
                                                {
                                                    component = productComponentAdds.First();
                                                }
                                            }
                                            else
                                            {
                                                component = templateComponents.FirstOrDefault(x =>
                                                    x.ComponentTypeId == type.Id
                                                    && x.Default == true
                                                );
                                            }
                                            if (component != null)
                                            {
                                                var productComponent =
                                                    productComponents.FirstOrDefault(x =>
                                                        x.ComponentId == component.Id
                                                    );
                                                if (
                                                    productComponent != null
                                                    && !string.IsNullOrEmpty(
                                                        productComponent.NoteImage
                                                    )
                                                )
                                                {
                                                    var listStringImage =
                                                        JsonConvert.DeserializeObject<List<string>>(
                                                            productComponent.NoteImage
                                                        );
                                                    var listImage = new List<string>();
                                                    if (
                                                        listStringImage != null
                                                        && listStringImage.Count > 0
                                                    )
                                                    {
                                                        var insideTask1s = new List<Task>();
                                                        foreach (var item in listStringImage)
                                                        {
                                                            insideTask1s.Add(
                                                                Task.Run(async () =>
                                                                {
                                                                    var image =
                                                                        JsonConvert.DeserializeObject<FileDTO>(
                                                                            item
                                                                        );
                                                                    listImage.Add(
                                                                        await Ultils.UploadImageBase64(
                                                                            wwwroot,
                                                                            $"Product/{product.Id}/Component/{component.Id}",
                                                                            image.Base64String,
                                                                            image.FileName,
                                                                            image.ContentType,
                                                                            null
                                                                        )
                                                                    );
                                                                })
                                                            );
                                                        }
                                                        await Task.WhenAll(insideTask1s);

                                                        productComponent.NoteImage =
                                                            JsonConvert.SerializeObject(listImage);
                                                    }
                                                }
                                                saveOrderComponents.Add(
                                                    new ProductComponent()
                                                    {
                                                        ComponentId = component.Id,
                                                        Id = Ultils.GenGuidString(),
                                                        LastestUpdatedTime =
                                                            DateTime.UtcNow.AddHours(7),
                                                        Name = component.Name,
                                                        Image = "",
                                                        ProductStageId = null,
                                                        Note = productComponent?.Note,
                                                        NoteImage = productComponent?.NoteImage
                                                    }
                                                );
                                            }
                                            else
                                            {
                                                throw new UserException(
                                                    $"Không tìm thấy kiểu bộ phận: {type.Name}"
                                                );
                                            }
                                        })
                                    );
                                }
                                await Task.WhenAll(insideTasks);
                            } while (saveOrderComponents.Count < templateComponentTypes.Count());
                        }
                        else
                        {
                            throw new UserException("Không tìm thấy kiểu bộ phận");
                        }

                        product.SaveOrderComponents = JsonConvert.SerializeObject(
                            saveOrderComponents
                        );
                    })
                );
                #endregion

                await Task.WhenAll(tasks);

                if (productRepository.Create(product))
                {
                    if (await orderService.CheckOrderPaid(product.OrderId))
                    {
                        try
                        {
                            if (
                                await productBodySizeService.UpdateProductBodySize(
                                    product.Id,
                                    template.Id,
                                    profileId,
                                    order.CustomerId
                                )
                            )
                            {
                                product.ReferenceProfileBodyId = profileId;

                                if (productRepository.Update(product.Id, product))
                                {
                                    if (addOrderMaterial != null)
                                    {
                                        return orderMaterialRepository.Create(addOrderMaterial)
                                            ? product.Id
                                            : null;
                                    }
                                    else if (updateOrderMaterial != null)
                                    {
                                        return orderMaterialRepository.Update(
                                            updateOrderMaterial.Id,
                                            updateOrderMaterial
                                        )
                                            ? product.Id
                                            : null;
                                    }
                                }
                                else
                                {
                                    throw new SystemsException(
                                        $"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo sản phẩm",
                                        nameof(ProductService)
                                    );
                                }
                            }
                            else
                            {
                                throw new SystemsException(
                                    $"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo số đo sản phẩm",
                                    nameof(ProductService)
                                );
                            }
                        }
                        catch (UserException uex)
                        {
                            product.IsActive = false;
                            product.InactiveTime = DateTime.UtcNow.AddHours(7);

                            if (productRepository.Update(product.Id, product))
                            {
                                throw new UserException(uex.Message);
                            }
                        }
                    }
                    else
                    {
                        throw new SystemsException(
                            $"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật hóa đơn",
                            nameof(ProductService)
                        );
                    }
                }
                else
                {
                    throw new SystemsException(
                        $"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo sản phẩm",
                        nameof(ProductService)
                    );
                }
                return null;
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<string> UpdateProduct(
            string wwwroot,
            string orderId,
            Product product,
            List<ProductComponent> productComponents,
            string materialId,
            string profileId,
            bool isCusMaterial,
            double materialQuantity
        )
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
                            throw new UserException(
                                "Sản phẩm trong giai đoạn thực hiện. Không thể chỉnh sửa"
                            );
                        }
                        else
                        {
                            if (product.ProductTemplateId != dbProduct.ProductTemplateId)
                            {
                                throw new UserException("Không thể thay đổi bản mẫu của sản phẩm");
                            }
                            else
                            {
                                #region GetData
                                var tasks = new List<Task>();
                                //bản mẫu
                                var template = productTemplateRepository.Get(
                                    product.ProductTemplateId
                                );

                                // các bộ phận của bản mẫu
                                var templateComponentTypes = componentTypeRepository.GetAll(x =>
                                    x.CategoryId == template.CategoryId && x.IsActive == true
                                );
                                if (templateComponentTypes != null && templateComponentTypes.Any())
                                {
                                    templateComponentTypes = templateComponentTypes.ToList();
                                }
                                else
                                {
                                    throw new UserException("Không tìm thấy bộ phận của bản mẫu");
                                }

                                // các kiểu bộ phận của bản mẫu
                                var templateComponents = componentRepository.GetAll(x =>
                                    x.ProductTemplateId == template.Id && x.IsActive == true
                                );
                                if (templateComponents != null && templateComponents.Any())
                                {
                                    templateComponents = templateComponents.ToList();
                                }
                                else
                                {
                                    throw new UserException(
                                        "Không tìm thấy kiểu bộ phận của bản mẫu"
                                    );
                                }

                                var material = materialRepository.Get(materialId);

                                var materialCategory = materialCategoryRepository.Get(
                                    material != null ? material.MaterialCategoryId : null
                                );

                                var sameProductMaterials = productRepository.GetAll(x =>
                                    x.OrderId == orderId
                                    && x.FabricMaterialId == materialId
                                    && x.IsActive == true
                                    && x.Status > 0
                                );
                                if (sameProductMaterials != null && sameProductMaterials.Any())
                                {
                                    sameProductMaterials = sameProductMaterials.ToList();
                                }
                                var sameProductMaterialdbs = productRepository.GetAll(x =>
                                    x.OrderId == orderId
                                    && x.FabricMaterialId == dbProduct.FabricMaterialId
                                    && x.IsActive == true
                                    && x.Status > 0
                                );
                                if (sameProductMaterialdbs != null && sameProductMaterialdbs.Any())
                                {
                                    sameProductMaterialdbs = sameProductMaterialdbs.ToList();
                                }

                                var orderMaterials = orderMaterialRepository.GetAll(x =>
                                    x.OrderId == orderId
                                    && (
                                        x.MaterialId == materialId
                                        || x.MaterialId == dbProduct.FabricMaterialId
                                    )
                                    && x.IsActive == true
                                );
                                if (orderMaterials != null && orderMaterials.Any())
                                {
                                    orderMaterials = orderMaterials.ToList();
                                }
                                var addOrderMaterial = new OrderMaterial();
                                var updateOrderMaterial = new OrderMaterial();
                                #endregion

                                tasks.Add(
                                    Task.Run(() =>
                                    {
                                        if (
                                            product.FabricMaterialId != null
                                            && product.FabricMaterialId
                                                != dbProduct.FabricMaterialId
                                        )
                                        {
                                            if (
                                                sameProductMaterialdbs == null
                                                || !sameProductMaterialdbs.Any(x =>
                                                    x.Id != product.Id
                                                )
                                            )
                                            {
                                                var oldMaterial = orderMaterials.FirstOrDefault(x =>
                                                    x.MaterialId == dbProduct.FabricMaterialId
                                                );
                                                if (oldMaterial != null)
                                                {
                                                    if (
                                                        !orderMaterialRepository.Delete(
                                                            oldMaterial.Id
                                                        )
                                                    )
                                                    {
                                                        throw new SystemsException(
                                                            "Lỗi trong quá trình xóa order materials",
                                                            nameof(ProductService)
                                                        );
                                                    }
                                                }

                                                if (
                                                    sameProductMaterials == null
                                                    || !sameProductMaterials.Any()
                                                )
                                                {
                                                    var orderMaterial = new OrderMaterial()
                                                    {
                                                        Id = Ultils.GenGuidString(),
                                                        MaterialId = materialId,
                                                        OrderId = orderId,
                                                        Value = (decimal)materialQuantity,
                                                        IsActive = true,
                                                        CreatedTime = DateTime.UtcNow.AddHours(7),
                                                        LastestUpdatedTime =
                                                            DateTime.UtcNow.AddHours(7),
                                                        IsCusMaterial = false,
                                                        Image = null,
                                                        InactiveTime = null
                                                    };

                                                    if (
                                                        !orderMaterialRepository.Create(
                                                            orderMaterial
                                                        )
                                                    )
                                                    {
                                                        throw new SystemsException(
                                                            "Lỗi trong quá trình tạo order materials",
                                                            nameof(ProductService)
                                                        );
                                                    }
                                                }
                                            }
                                        }
                                    })
                                );

                                tasks.Add(
                                    Task.Run(() =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(product.Name))
                                        {
                                            dbProduct.Name = product.Name;
                                        }
                                    })
                                );

                                tasks.Add(
                                    Task.Run(() =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(product.Note))
                                        {
                                            dbProduct.Note = product.Note;
                                        }
                                    })
                                );

                                tasks.Add(
                                    Task.Run(() =>
                                    {
                                        dbProduct.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                    })
                                );

                                //    tasks.Add(
                                //        Task.Run(() =>
                                //        {
                                //            if (isCusMaterial)
                                //            {
                                //                dbProduct.Price = template.Price;
                                //            }
                                //            else
                                //            {
                                //                dbProduct.Price =
                                //                    template.Price
                                //                    + Math.Abs(
                                //                        Math.Round(
                                //(decimal)(
                                //    (double)(
                                //        materialCategory != null
                                //            ? materialCategory.PricePerUnit
                                //            : 0
                                //    ) * materialQuantity
                                //),
                                //2
                                //                        )
                                //                    );
                                //            }
                                //        })
                                //    );

                                tasks.Add(
                                    Task.Run(async () =>
                                    {
                                        if (!string.IsNullOrEmpty(dbProduct.SaveOrderComponents))
                                        {
                                            var newSaveOrderComponents =
                                                new List<ProductComponent>();
                                            var saveOrderComponents = JsonConvert.DeserializeObject<
                                                List<ProductComponent>
                                            >(dbProduct.SaveOrderComponents);

                                            if (
                                                saveOrderComponents != null
                                                && saveOrderComponents.Any()
                                            )
                                            {
                                                if (
                                                    templateComponentTypes != null
                                                    && templateComponentTypes.Any()
                                                )
                                                {
                                                    foreach (
                                                        var templateComponentType in templateComponentTypes
                                                    )
                                                    {
                                                        var components = templateComponents.Where(
                                                            x =>
                                                                x.ComponentTypeId
                                                                == templateComponentType.Id
                                                        );
                                                        if (components != null)
                                                        {
                                                            var saveOrderComponent =
                                                                saveOrderComponents.FirstOrDefault(
                                                                    x =>
                                                                        components
                                                                            .Select(c => c.Id)
                                                                            .Contains(x.ComponentId)
                                                                );
                                                            var newOrderComponent =
                                                                productComponents.FirstOrDefault(
                                                                    x =>
                                                                        components
                                                                            .Select(c => c.Id)
                                                                            .Contains(x.ComponentId)
                                                                );
                                                            if (
                                                                saveOrderComponent == null
                                                                && newOrderComponent == null
                                                            )
                                                            {
                                                                var defaultComponent =
                                                                    components.FirstOrDefault(x =>
                                                                        x.Default == true
                                                                    );

                                                                if (defaultComponent == null)
                                                                {
                                                                    defaultComponent =
                                                                        components.First();
                                                                }

                                                                newSaveOrderComponents.Add(
                                                                    new ProductComponent()
                                                                    {
                                                                        Id = Ultils.GenGuidString(),
                                                                        ComponentId =
                                                                            defaultComponent.Id,
                                                                        Name =
                                                                            defaultComponent.Name,
                                                                        Image = "",
                                                                        ProductStageId = null,
                                                                        LastestUpdatedTime =
                                                                            DateTime.UtcNow.AddHours(
                                                                                7
                                                                            ),
                                                                        Note = null,
                                                                        NoteImage = null
                                                                    }
                                                                );
                                                            }
                                                            else if (
                                                                saveOrderComponent != null
                                                                && newOrderComponent == null
                                                            )
                                                            {
                                                                newSaveOrderComponents.Add(
                                                                    saveOrderComponent
                                                                );
                                                            }
                                                            else if (
                                                                saveOrderComponent == null
                                                                && newOrderComponent != null
                                                            )
                                                            {
                                                                var selectedComponent =
                                                                    components.FirstOrDefault(x =>
                                                                        x.Id
                                                                        == newOrderComponent.ComponentId
                                                                    );
                                                                if (selectedComponent != null)
                                                                {
                                                                    if (
                                                                        !string.IsNullOrEmpty(
                                                                            newOrderComponent.NoteImage
                                                                        )
                                                                    )
                                                                    {
                                                                        var listStringImage =
                                                                            JsonConvert.DeserializeObject<
                                                                                List<string>
                                                                            >(
                                                                                newOrderComponent.NoteImage
                                                                            );
                                                                        var listImage =
                                                                            new List<string>();
                                                                        if (
                                                                            listStringImage != null
                                                                            && listStringImage.Count
                                                                                > 0
                                                                        )
                                                                        {
                                                                            var insideTask1s =
                                                                                new List<Task>();
                                                                            foreach (
                                                                                var item in listStringImage
                                                                            )
                                                                            {
                                                                                insideTask1s.Add(
                                                                                    Task.Run(
                                                                                        async () =>
                                                                                        {
                                                                                            var image =
                                                                                                JsonConvert.DeserializeObject<FileDTO>(
                                                                                                    item
                                                                                                );
                                                                                            listImage.Add(
                                                                                                await Ultils.UploadImageBase64(
                                                                                                    wwwroot,
                                                                                                    $"Product/{product.Id}/Component/{selectedComponent.Id}",
                                                                                                    image.Base64String,
                                                                                                    image.FileName,
                                                                                                    image.ContentType,
                                                                                                    null
                                                                                                )
                                                                                            );
                                                                                        }
                                                                                    )
                                                                                );
                                                                            }
                                                                            await Task.WhenAll(
                                                                                insideTask1s
                                                                            );

                                                                            newOrderComponent.NoteImage =
                                                                                JsonConvert.SerializeObject(
                                                                                    listImage
                                                                                );
                                                                        }
                                                                    }
                                                                    newSaveOrderComponents.Add(
                                                                        new ProductComponent()
                                                                        {
                                                                            Id =
                                                                                Ultils.GenGuidString(),
                                                                            ComponentId =
                                                                                selectedComponent.Id,
                                                                            Name =
                                                                                selectedComponent.Name,
                                                                            Image = "",
                                                                            ProductStageId = null,
                                                                            LastestUpdatedTime =
                                                                                DateTime.UtcNow.AddHours(
                                                                                    7
                                                                                ),
                                                                            Note =
                                                                                newOrderComponent.Note,
                                                                            NoteImage =
                                                                                newOrderComponent.NoteImage
                                                                        }
                                                                    );
                                                                }
                                                                else
                                                                {
                                                                    throw new UserException(
                                                                        $"Không tìm thấy kiểu bộ phận: {templateComponentType.Name}"
                                                                    );
                                                                }
                                                            }
                                                            else if (
                                                                saveOrderComponent != null
                                                                && newOrderComponent != null
                                                            )
                                                            {
                                                                var selectedComponent =
                                                                    components.FirstOrDefault(x =>
                                                                        x.Id
                                                                        == newOrderComponent.ComponentId
                                                                    );
                                                                if (selectedComponent != null)
                                                                {
                                                                    if (
                                                                        saveOrderComponent.ComponentId
                                                                        != selectedComponent.Id
                                                                    )
                                                                    {
                                                                        saveOrderComponent.ComponentId =
                                                                            selectedComponent.Id;
                                                                        saveOrderComponent.Name =
                                                                            selectedComponent.Name;
                                                                    }
                                                                    saveOrderComponent.LastestUpdatedTime =
                                                                        DateTime.UtcNow.AddHours(7);
                                                                    saveOrderComponent.Note =
                                                                        newOrderComponent.Note;

                                                                    if (
                                                                        !string.IsNullOrEmpty(
                                                                            newOrderComponent.NoteImage
                                                                        )
                                                                    )
                                                                    {
                                                                        var listImage =
                                                                            new List<string>();

                                                                        var listStringImage =
                                                                            JsonConvert.DeserializeObject<
                                                                                List<string>
                                                                            >(
                                                                                newOrderComponent.NoteImage
                                                                            );
                                                                        if (
                                                                            listStringImage != null
                                                                            && listStringImage.Any()
                                                                        )
                                                                        {
                                                                            var listOldImageUrl =
                                                                                listStringImage
                                                                                    .Where(x =>
                                                                                        x.Contains(
                                                                                            "http"
                                                                                        )
                                                                                    )
                                                                                    .ToList();
                                                                            if (
                                                                                listOldImageUrl
                                                                                    != null
                                                                                && listOldImageUrl.Count
                                                                                    > 0
                                                                            )
                                                                            {
                                                                                if (
                                                                                    !string.IsNullOrEmpty(
                                                                                        saveOrderComponent.NoteImage
                                                                                    )
                                                                                )
                                                                                {
                                                                                    var listOldImage =
                                                                                        JsonConvert.DeserializeObject<
                                                                                            List<string>
                                                                                        >(
                                                                                            saveOrderComponent.NoteImage
                                                                                        );
                                                                                    if (
                                                                                        listOldImage
                                                                                            != null
                                                                                        && listOldImage.Count
                                                                                            > 0
                                                                                    )
                                                                                    {
                                                                                        var checkOldImageTasks =
                                                                                            new List<Task>();
                                                                                        foreach (
                                                                                            var item in listOldImage
                                                                                        )
                                                                                        {
                                                                                            checkOldImageTasks.Add(
                                                                                                Task.Run(
                                                                                                    () =>
                                                                                                    {
                                                                                                        var imageObject =
                                                                                                            JsonConvert.DeserializeObject<ImageFileDTO>(
                                                                                                                item
                                                                                                            );
                                                                                                        if (
                                                                                                            listOldImageUrl.Contains(
                                                                                                                imageObject.ObjectUrl
                                                                                                            )
                                                                                                        )
                                                                                                        {
                                                                                                            listImage.Add(
                                                                                                                item
                                                                                                            );
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            Ultils.DeleteObject(
                                                                                                                imageObject.ObjectName
                                                                                                            );
                                                                                                        }
                                                                                                    }
                                                                                                )
                                                                                            );
                                                                                        }
                                                                                        await Task.WhenAll(
                                                                                            checkOldImageTasks
                                                                                        );
                                                                                    }
                                                                                }
                                                                            }

                                                                            var listNewImageFile =
                                                                                listStringImage
                                                                                    .Where(x =>
                                                                                        !x.Contains(
                                                                                            "http"
                                                                                        )
                                                                                    )
                                                                                    .ToList();
                                                                            if (
                                                                                listNewImageFile
                                                                                    != null
                                                                                && listNewImageFile.Count
                                                                                    > 0
                                                                            )
                                                                            {
                                                                                var insideTask1s =
                                                                                    new List<Task>();
                                                                                foreach (
                                                                                    var item in listNewImageFile
                                                                                )
                                                                                {
                                                                                    insideTask1s.Add(
                                                                                        Task.Run(
                                                                                            async () =>
                                                                                            {
                                                                                                var image =
                                                                                                    JsonConvert.DeserializeObject<FileDTO>(
                                                                                                        item
                                                                                                    );
                                                                                                listImage.Add(
                                                                                                    await Ultils.UploadImageBase64(
                                                                                                        wwwroot,
                                                                                                        $"Product/{product.Id}/Component/{selectedComponent.Id}",
                                                                                                        image.Base64String,
                                                                                                        image.FileName,
                                                                                                        image.ContentType,
                                                                                                        null
                                                                                                    )
                                                                                                );
                                                                                            }
                                                                                        )
                                                                                    );
                                                                                }
                                                                                await Task.WhenAll(
                                                                                    insideTask1s
                                                                                );
                                                                            }
                                                                        }
                                                                        if (listImage.Count > 0)
                                                                        {
                                                                            saveOrderComponent.NoteImage =
                                                                                JsonConvert.SerializeObject(
                                                                                    listImage
                                                                                );
                                                                        }
                                                                        else
                                                                        {
                                                                            saveOrderComponent.NoteImage =
                                                                                null;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        if (
                                                                            !string.IsNullOrEmpty(
                                                                                saveOrderComponent.NoteImage
                                                                            )
                                                                        )
                                                                        {
                                                                            var listOldImage =
                                                                                JsonConvert.DeserializeObject<
                                                                                    List<string>
                                                                                >(
                                                                                    saveOrderComponent.NoteImage
                                                                                );
                                                                            if (
                                                                                listOldImage != null
                                                                                && listOldImage.Count
                                                                                    > 0
                                                                            )
                                                                            {
                                                                                var deleteOldImageTasks =
                                                                                    new List<Task>();
                                                                                foreach (
                                                                                    var item in listOldImage
                                                                                )
                                                                                {
                                                                                    deleteOldImageTasks.Add(
                                                                                        Task.Run(
                                                                                            () =>
                                                                                            {
                                                                                                var imageObject =
                                                                                                    JsonConvert.DeserializeObject<ImageFileDTO>(
                                                                                                        item
                                                                                                    );
                                                                                                Ultils.DeleteObject(
                                                                                                    imageObject.ObjectName
                                                                                                );
                                                                                            }
                                                                                        )
                                                                                    );
                                                                                }
                                                                                await Task.WhenAll(
                                                                                    deleteOldImageTasks
                                                                                );
                                                                            }
                                                                        }
                                                                        saveOrderComponent.NoteImage =
                                                                            null;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    throw new UserException(
                                                                        $"Không tìm thấy kiểu bộ phận: {templateComponentType.Name}"
                                                                    );
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            throw new UserException(
                                                                $"Không tìm thấy kiểu bộ phận: {templateComponentType.Name}"
                                                            );
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    throw new UserException(
                                                        "Không tìm thấy kiểu bộ phận"
                                                    );
                                                }

                                                dbProduct.SaveOrderComponents =
                                                    JsonConvert.SerializeObject(
                                                        saveOrderComponents
                                                    );
                                            }
                                            else
                                            {
                                                saveOrderComponents = new List<ProductComponent>();

                                                if (
                                                    templateComponentTypes != null
                                                    && templateComponentTypes.Any()
                                                )
                                                {
                                                    do
                                                    {
                                                        saveOrderComponents =
                                                            new List<ProductComponent>();
                                                        var insideTasks = new List<Task>();
                                                        foreach (var type in templateComponentTypes)
                                                        {
                                                            insideTasks.Add(
                                                                Task.Run(async () =>
                                                                {
                                                                    var componentIds = string.Join(
                                                                        ",",
                                                                        productComponents.Select(
                                                                            c => c.ComponentId
                                                                        )
                                                                    );
                                                                    var productComponentAdds =
                                                                        templateComponents.Where(
                                                                            x =>
                                                                                x.ComponentTypeId
                                                                                    == type.Id
                                                                                && componentIds.Contains(
                                                                                    x.Id
                                                                                )
                                                                        );
                                                                    var component = new Component();
                                                                    if (
                                                                        productComponentAdds != null
                                                                        && productComponentAdds.Any()
                                                                    )
                                                                    {
                                                                        if (
                                                                            productComponentAdds.Count()
                                                                            > 1
                                                                        )
                                                                        {
                                                                            throw new UserException(
                                                                                "Chỉ được chọn 1 kiểu cho 1 bộ phận"
                                                                            );
                                                                        }
                                                                        else
                                                                        {
                                                                            component =
                                                                                productComponentAdds.First();
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        component =
                                                                            templateComponents.FirstOrDefault(
                                                                                x =>
                                                                                    x.ComponentTypeId
                                                                                        == type.Id
                                                                                    && x.Default
                                                                                        == true
                                                                            );
                                                                    }

                                                                    if (component != null)
                                                                    {
                                                                        var productComponent =
                                                                            productComponents.FirstOrDefault(
                                                                                x =>
                                                                                    x.ComponentId
                                                                                    == component.Id
                                                                            );
                                                                        if (
                                                                            productComponent != null
                                                                            && !string.IsNullOrEmpty(
                                                                                productComponent.NoteImage
                                                                            )
                                                                        )
                                                                        {
                                                                            var listStringImage =
                                                                                JsonConvert.DeserializeObject<
                                                                                    List<string>
                                                                                >(
                                                                                    productComponent.NoteImage
                                                                                );
                                                                            var listImage =
                                                                                new List<string>();
                                                                            if (
                                                                                listStringImage
                                                                                    != null
                                                                                && listStringImage.Count
                                                                                    > 0
                                                                            )
                                                                            {
                                                                                var insideTask1s =
                                                                                    new List<Task>();
                                                                                foreach (
                                                                                    var item in listStringImage
                                                                                )
                                                                                {
                                                                                    insideTask1s.Add(
                                                                                        Task.Run(
                                                                                            async () =>
                                                                                            {
                                                                                                var image =
                                                                                                    JsonConvert.DeserializeObject<FileDTO>(
                                                                                                        item
                                                                                                    );
                                                                                                listImage.Add(
                                                                                                    await Ultils.UploadImageBase64(
                                                                                                        wwwroot,
                                                                                                        $"Product/{product.Id}/Component/{component.Id}",
                                                                                                        image.Base64String,
                                                                                                        image.FileName,
                                                                                                        image.ContentType,
                                                                                                        null
                                                                                                    )
                                                                                                );
                                                                                            }
                                                                                        )
                                                                                    );
                                                                                }
                                                                                await Task.WhenAll(
                                                                                    insideTask1s
                                                                                );

                                                                                productComponent.NoteImage =
                                                                                    JsonConvert.SerializeObject(
                                                                                        listImage
                                                                                    );
                                                                            }
                                                                        }
                                                                        saveOrderComponents.Add(
                                                                            new ProductComponent()
                                                                            {
                                                                                ComponentId =
                                                                                    component.Id,
                                                                                Id =
                                                                                    Ultils.GenGuidString(),
                                                                                LastestUpdatedTime =
                                                                                    DateTime.UtcNow.AddHours(
                                                                                        7
                                                                                    ),
                                                                                Name =
                                                                                    component.Name,
                                                                                Image = "",
                                                                                ProductStageId =
                                                                                    null,
                                                                                Note =
                                                                                    productComponent?.Note,
                                                                                NoteImage =
                                                                                    productComponent?.NoteImage
                                                                            }
                                                                        );
                                                                    }
                                                                    else
                                                                    {
                                                                        throw new UserException(
                                                                            $"Không tìm thấy kiểu bộ phận: {type.Name}"
                                                                        );
                                                                    }
                                                                })
                                                            );
                                                        }
                                                        await Task.WhenAll(insideTasks);
                                                    } while (
                                                        saveOrderComponents.Count
                                                        != templateComponentTypes.Count()
                                                    );
                                                }
                                                else
                                                {
                                                    throw new UserException(
                                                        "Không tìm thấy kiểu bộ phận"
                                                    );
                                                }

                                                dbProduct.SaveOrderComponents =
                                                    JsonConvert.SerializeObject(
                                                        saveOrderComponents
                                                    );
                                            }
                                        }
                                    })
                                );

                                await Task.WhenAll(tasks);

                                if (productRepository.Update(dbProduct.Id, dbProduct))
                                {
                                    if (await orderService.CheckOrderPaid(dbProduct.OrderId))
                                    {
                                        if (
                                            await productBodySizeService.UpdateProductBodySize(
                                                product.Id,
                                                template.Id,
                                                profileId,
                                                dbOrder.CustomerId
                                            )
                                        )
                                        {
                                            product.ReferenceProfileBodyId = profileId;

                                            return productRepository.Update(dbProduct.Id, dbProduct)
                                                ? dbProduct.Id
                                                : null;
                                        }
                                        else
                                        {
                                            throw new SystemsException(
                                                $"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo số đo sản phẩm",
                                                nameof(ProductService)
                                            );
                                        }
                                    }
                                    else
                                    {
                                        throw new SystemsException(
                                            $"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật hóa đơn",
                                            nameof(ProductService)
                                        );
                                    }
                                }
                                else
                                {
                                    throw new SystemsException(
                                        $"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật sản phẩm",
                                        nameof(ProductService)
                                    );
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

        public bool UpdateProductPrice(string orderId, string productId, decimal? price)
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
                    var dbProduct = productRepository.Get(productId);
                    if (dbProduct != null && dbProduct.IsActive == true && dbOrder.Id == dbProduct.OrderId)
                    {
                        if (dbProduct.Status >= 2)
                        {
                            throw new UserException(
                                "Sản phẩm trong giai đoạn thực hiện. Không thể chỉnh sửa"
                            );
                        }
                        else
                        {
                            if (price == null || price < 0)
                            {
                                throw new UserException("Giá không hợp lệ");
                            }
                            else
                            {
                                dbProduct.Price = price;

                                return productRepository.Update(dbProduct.Id, dbProduct);
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
                var order = orderRepository.Get(dbProduct.OrderId);
                if (order != null)
                {
                    if (order.Status >= 2)
                    {
                        switch (order.Status)
                        {
                            case 0:
                                throw new UserException("Đơn hàng đã hủy. Không thể xóa sản phẩm");
                            case 2:
                                throw new UserException(
                                    "Đơn hàng đã duyệt. Không thể xóa sản phẩm"
                                );
                            case 3:
                                throw new UserException(
                                    "Đơn hàng đã vào giai đoạn thực hiện. Không thể xóa sản phẩm"
                                );
                            case 4:
                                throw new UserException(
                                    "Đơn hàng đang trong giai đoạn thực hiện. Không thể xóa sản phẩm"
                                );
                            case 5:
                                throw new UserException(
                                    "Đơn hàng đã vào giai đoạn hoàn thiện. Không thể xóa sản phẩm"
                                );
                            case 6:
                                throw new UserException(
                                    "Đơn hàng đang chờ khách hàng kiểm thử. Không thể xóa sản phẩm"
                                );
                            case 7:
                                throw new UserException(
                                    "Đơn hàng đã bàn giao cho khách. Không thể xóa sản phẩm"
                                );
                            default:
                                throw new UserException("Đơn hàng đã hủy. Không thể xóa sản phẩm");
                        }
                    }
                    else if (order.PaidMoney > 0 || order.Deposit > 0)
                    {
                        throw new UserException("Đơn hàng đã thanh toán. Không thể xóa sản phẩm");
                    }
                    else
                    {
                        dbProduct.CreatedTime = null;
                        dbProduct.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                        dbProduct.IsActive = false;
                        dbProduct.InactiveTime = DateTime.UtcNow.AddHours(7);

                        if (productRepository.Update(dbProduct.Id, dbProduct))
                        {
                            if (await orderService.CheckOrderPaid(dbProduct.OrderId))
                            {
                                return true;
                            }
                            else
                            {
                                throw new SystemsException(
                                    $"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật hóa đơn",
                                    nameof(ProductService)
                                );
                            }
                        }
                        else
                        {
                            throw new SystemsException(
                                $"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật sản phẩm",
                                nameof(ProductService)
                            );
                        }
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy hóa đơn");
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
                        product.ProductTemplate = productTemplateRepository.Get(
                            product.ProductTemplateId
                        );
                    }
                    if (!string.IsNullOrEmpty(product.FabricMaterialId))
                    {
                        product.FabricMaterial = materialRepository.Get(product.FabricMaterialId);
                    }
                    if (product.ProductTemplate != null)
                    {
                        product.ProductTemplate.ThumbnailImage = Ultils.GetUrlImage(
                            product.ProductTemplate.ThumbnailImage
                        );
                    }
                    return product;
                }
            }
            return null;
        }

        public async Task<Product> GetProductOrderByCus(string id, string orderId, string cusId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (
                dbOrder != null
                && dbOrder.IsActive == true
                && dbOrder.Status >= 1
                && dbOrder.CustomerId == cusId
            )
            {
                var product = productRepository.Get(id);
                if (product != null && product.IsActive == true)
                {
                    if (!string.IsNullOrEmpty(product.ProductTemplateId))
                    {
                        product.ProductTemplate = productTemplateRepository.Get(
                            product.ProductTemplateId
                        );
                    }
                    if (!string.IsNullOrEmpty(product.FabricMaterialId))
                    {
                        product.FabricMaterial = materialRepository.Get(product.FabricMaterialId);
                    }
                    await Task.WhenAll(
                        Task.Run(() =>
                        {
                            if (
                                product.ProductTemplate != null
                                && !string.IsNullOrEmpty(product.ProductTemplate.ThumbnailImage)
                            )
                            {
                                product.ProductTemplate.ThumbnailImage = Ultils.GetUrlImage(
                                    product.ProductTemplate.ThumbnailImage
                                );
                            }
                        }),
                        Task.Run(() =>
                        {
                            if (
                                product.FabricMaterial != null
                                && !string.IsNullOrEmpty(product.FabricMaterial.Image)
                            )
                            {
                                product.FabricMaterial.Image = Ultils.GetUrlImage(
                                    product.FabricMaterial.Image
                                );
                            }
                        })
                    );
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
                var products = productRepository
                    .GetAll(x => x.OrderId == orderId && x.IsActive == true)
                    .ToList();

                if (products.Any() && products.Count() > 0)
                {
                    products = products.ToList();

                    var templates = productTemplateRepository.GetAll(x =>
                        products.Select(p => p.ProductTemplateId).Contains(x.Id)
                    );
                    if (templates != null && templates.Any())
                    {
                        templates = templates.ToList();

                        var fabricMaterials = materialRepository.GetAll(x =>
                            products.Select(p => p.FabricMaterialId).Contains(x.Id)
                        );

                        if (fabricMaterials != null && fabricMaterials.Any())
                        {
                            fabricMaterials = fabricMaterials.ToList();

                            var tasks = new List<Task>();

                            foreach (var product in products)
                            {
                                tasks.Add(
                                    Task.Run(async () =>
                                    {
                                        var setValueTasks = new List<Task>();
                                        setValueTasks.Add(
                                            Task.Run(async () =>
                                            {
                                                product.ProductTemplate = templates.SingleOrDefault(
                                                    x => x.Id == product.ProductTemplateId
                                                );
                                                if (
                                                    product.ProductTemplate != null
                                                    && !string.IsNullOrEmpty(
                                                        product.ProductTemplate.ThumbnailImage
                                                    )
                                                )
                                                {
                                                    product.ProductTemplate.ThumbnailImage =
                                                        Ultils.GetUrlImage(
                                                            product.ProductTemplate.ThumbnailImage
                                                        );
                                                }
                                            })
                                        );
                                        setValueTasks.Add(
                                            Task.Run(async () =>
                                            {
                                                product.FabricMaterial =
                                                    fabricMaterials.SingleOrDefault(x =>
                                                        x.Id == product.FabricMaterialId
                                                    );
                                                if (
                                                    product.FabricMaterial != null
                                                    && !string.IsNullOrEmpty(
                                                        product.FabricMaterial.Image
                                                    )
                                                )
                                                {
                                                    product.FabricMaterial.Image =
                                                        Ultils.GetUrlImage(
                                                            product.FabricMaterial.Image
                                                        );
                                                }
                                            })
                                        );

                                        await Task.WhenAll(setValueTasks);
                                    })
                                );
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
                var products = productRepository.GetAll(x =>
                    dbOrders.Select(o => o.Id).Contains(x.OrderId) && x.IsActive == true
                );

                if (products.Any() && products.Count() > 0)
                {
                    products = products.ToList();

                    var templates = productTemplateRepository.GetAll(x =>
                        products.Select(p => p.ProductTemplateId).Contains(x.Id)
                    );
                    if (templates != null && templates.Any())
                    {
                        templates = templates.ToList();

                        var fabricMaterials = materialRepository.GetAll(x =>
                            products.Select(p => p.FabricMaterialId).Contains(x.Id)
                        );

                        if (fabricMaterials != null && fabricMaterials.Any())
                        {
                            fabricMaterials = fabricMaterials.ToList();

                            var tasks = new List<Task>();

                            foreach (var product in products)
                            {
                                tasks.Add(
                                    Task.Run(async () =>
                                    {
                                        var setValueTasks = new List<Task>();
                                        setValueTasks.Add(
                                            Task.Run(async () =>
                                            {
                                                product.ProductTemplate = templates.SingleOrDefault(
                                                    x => x.Id == product.ProductTemplateId
                                                );
                                                if (
                                                    product.ProductTemplate != null
                                                    && !string.IsNullOrEmpty(
                                                        product.ProductTemplate.ThumbnailImage
                                                    )
                                                )
                                                {
                                                    product.ProductTemplate.ThumbnailImage =
                                                        Ultils.GetUrlImage(
                                                            product.ProductTemplate.ThumbnailImage
                                                        );
                                                }
                                            })
                                        );
                                        setValueTasks.Add(
                                            Task.Run(async () =>
                                            {
                                                product.FabricMaterial =
                                                    fabricMaterials.SingleOrDefault(x =>
                                                        x.Id == product.FabricMaterialId
                                                    );
                                                if (
                                                    product.FabricMaterial != null
                                                    && !string.IsNullOrEmpty(
                                                        product.FabricMaterial.Image
                                                    )
                                                )
                                                {
                                                    product.FabricMaterial.Image =
                                                        Ultils.GetUrlImage(
                                                            product.FabricMaterial.Image
                                                        );
                                                }
                                            })
                                        );

                                        await Task.WhenAll(setValueTasks);
                                    })
                                );
                            }

                            await Task.WhenAll(tasks);

                            return products;
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Product>> GetProductsByOrderIdOfCus(
            string orderId,
            string cusId
        )
        {
            var dbOrder = orderRepository.Get(orderId);
            if (
                dbOrder != null
                && dbOrder.CustomerId == cusId
                && dbOrder.IsActive == true
                && dbOrder.Status >= 1
            )
            {
                var products = productRepository
                    .GetAll(x => x.OrderId == orderId && x.IsActive == true)
                    .ToList();

                if (products.Any() && products.Count > 0)
                {
                    var templates = productTemplateRepository
                        .GetAll(x => products.Select(p => p.ProductTemplateId).Contains(x.Id))
                        .ToList();
                    var fabricMaterials = materialRepository
                        .GetAll(x => products.Select(p => p.FabricMaterialId).Contains(x.Id))
                        .ToList();

                    var tasks = new List<Task>();

                    foreach (var product in products)
                    {
                        tasks.Add(
                            Task.Run(async () =>
                            {
                                var setValueTasks = new List<Task>();
                                setValueTasks.Add(
                                    Task.Run(async () =>
                                    {
                                        product.ProductTemplate = templates.SingleOrDefault(x =>
                                            x.Id == product.ProductTemplateId
                                        );
                                        if (
                                            product.ProductTemplate != null
                                            && !string.IsNullOrEmpty(
                                                product.ProductTemplate.ThumbnailImage
                                            )
                                        )
                                        {
                                            product.ProductTemplate.ThumbnailImage =
                                                Ultils.GetUrlImage(
                                                    product.ProductTemplate.ThumbnailImage
                                                );
                                        }
                                    })
                                );
                                setValueTasks.Add(
                                    Task.Run(async () =>
                                    {
                                        product.FabricMaterial = fabricMaterials.SingleOrDefault(
                                            x => x.Id == product.FabricMaterialId
                                        );
                                        if (
                                            product.FabricMaterial != null
                                            && !string.IsNullOrEmpty(product.FabricMaterial.Image)
                                        )
                                        {
                                            product.FabricMaterial.Image = Ultils.GetUrlImage(
                                                product.FabricMaterial.Image
                                            );
                                        }
                                    })
                                );

                                await Task.WhenAll(setValueTasks);
                            })
                        );
                    }

                    await Task.WhenAll(tasks);

                    return products;
                }
            }
            return null;
        }
    }
}
