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
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Google.Api.Gax.ResourceNames;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Component = Etailor.API.Repository.EntityModels.Component;

namespace Etailor.API.Service.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductBodySizeService productBodySizeService;
        private readonly IProductBodySizeRepository productBodySizeRepository;
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
        private readonly IBodySizeRepository bodySizeRepository;

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
            IMasteryRepository masteryRepository,
            IProductBodySizeRepository productBodySizeRepository,
            IBodySizeRepository bodySizeRepository
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
            this.productBodySizeRepository = productBodySizeRepository;
            this.bodySizeRepository = bodySizeRepository;
        }

        public async Task<string> AddProduct(string wwwroot, string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity, int quantity)
        {
            var i = 0;

            var listParams = new List<List<SqlParameter>>();
            var tasks = new List<Task>();
            var templateId = new SqlParameter("@ProductTemplateId", System.Data.SqlDbType.NVarChar)
            {
                Value = product.ProductTemplateId != null ? product.ProductTemplateId : DBNull.Value
            };

            //các bộ phận của bản mẫu
            var templateComponentTypes = componentTypeRepository.GetStoreProcedure(StoreProcName.Get_Template_Component_Types, templateId);
            if (templateComponentTypes != null && templateComponentTypes.Any())
            {
                templateComponentTypes = templateComponentTypes.ToList();
            }

            //các kiểu bộ phận của bản mẫu
            var templateComponents = componentRepository.GetStoreProcedure(StoreProcName.Get_Template_Components, templateId);
            if (templateComponents != null && templateComponents.Any())
            {
                templateComponents = templateComponents.ToList();
            }

            do
            {
                tasks.Add(Task.Run(async () =>
                {
                    #region InitProduct
                    var id = Ultils.GenGuidString();

                    try
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
                                            var componentIds = string.Join(",", productComponents.Select(c => c.ComponentId));
                                            var productComponentAdds = templateComponents.Where(x => x.ComponentTypeId == type.Id && componentIds.Contains(x.Id));
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
                                                component = templateComponents.FirstOrDefault(x => x.ComponentTypeId == type.Id && x.Default == true);
                                            }
                                            if (component != null)
                                            {
                                                var productComponent = productComponents.FirstOrDefault(x => x.ComponentId == component.Id);
                                                if (productComponent != null && !string.IsNullOrEmpty(productComponent.NoteImage))
                                                {
                                                    var listStringImage = JsonConvert.DeserializeObject<List<string>>(productComponent.NoteImage);
                                                    var listImage = new List<string>();
                                                    if (listStringImage != null && listStringImage.Count > 0)
                                                    {
                                                        var insideTask1s = new List<Task>();
                                                        foreach (var item in listStringImage)
                                                        {
                                                            insideTask1s.Add(Task.Run(async () =>
                                                            {
                                                                var image = JsonConvert.DeserializeObject<FileDTO>(item);
                                                                if (!string.IsNullOrEmpty(image.Base64String) && !string.IsNullOrEmpty(image.FileName) && !string.IsNullOrEmpty(image.ContentType))
                                                                {
                                                                    listImage.Add(
                                                                        await Ultils.UploadImageBase64(
                                                                            wwwroot,
                                                                            $"Product/{id}/Component/{component.Id}",
                                                                            image.Base64String,
                                                                            image.FileName,
                                                                            image.ContentType,
                                                                            null));
                                                                }
                                                            }));
                                                        }
                                                        await Task.WhenAll(insideTask1s);

                                                        productComponent.NoteImage = JsonConvert.SerializeObject(listImage);
                                                    }
                                                }
                                                saveOrderComponents.Add(new ProductComponent()
                                                {
                                                    ComponentId = component.Id,
                                                    Id = Ultils.GenGuidString(),
                                                    LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                                    Name = component.Name,
                                                    Image = "",
                                                    ProductStageId = null,
                                                    Note = productComponent?.Note,
                                                    NoteImage = productComponent?.NoteImage
                                                });
                                            }
                                            else
                                            {
                                                throw new UserException($"Không tìm thấy kiểu bộ phận: {type.Name}");
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

                        product.SaveOrderComponents = JsonConvert.SerializeObject(saveOrderComponents);
                    }
                    catch (Exception ex)
                    {
                        throw new UserException(ex.Message);
                    }
                    #endregion

                    #region SetupProcParam
                    listParams.Add(new List<SqlParameter>()
                    {
                        new SqlParameter("@OrderId", System.Data.SqlDbType.NVarChar)
                    {
                        Value = orderId
                    },new SqlParameter("@ProductId", System.Data.SqlDbType.NVarChar)
                    {
                        Value = id
                    },new SqlParameter("@ProductTemplateId", System.Data.SqlDbType.NVarChar)
                    {
                        Value = product.ProductTemplateId != null ? product.ProductTemplateId : DBNull.Value
                    },new SqlParameter("@FabricMaterialId", System.Data.SqlDbType.NVarChar)
                    {
                        Value = materialId
                    },new SqlParameter("@MaterialValue", System.Data.SqlDbType.Decimal)
                    {
                        Value = materialQuantity != null ? materialQuantity : 0
                    },new SqlParameter("@IsCusMaterial", System.Data.SqlDbType.Bit)
                    {
                        Value = isCusMaterial ? 1 : 0
                    },new SqlParameter("@ProductName", System.Data.SqlDbType.NVarChar)
                    {
                        Value = !string.IsNullOrWhiteSpace(product.Name) ? product.Name : DBNull.Value
                    },new SqlParameter("@Note", System.Data.SqlDbType.NVarChar)
                    {
                        Value = !string.IsNullOrWhiteSpace(product.Note) ? product.Note : DBNull.Value
                    },new SqlParameter("@SaveOrderComponents", System.Data.SqlDbType.NVarChar)
                    {
                        Value = !string.IsNullOrWhiteSpace(product.SaveOrderComponents) ? product.SaveOrderComponents : DBNull.Value
                    },new SqlParameter("@ProfileBodyId", System.Data.SqlDbType.NVarChar)
                    {
                        Value = profileId
                    }
                    });
                    #endregion
                }));

                i++;
            } while (i < quantity);

            await Task.WhenAll(tasks);

            var results = new List<int>();


            foreach (var param in listParams)
            {
                try
                {
                    var result = await productRepository.GetStoreProcedureReturnInt("AddProduct"
                        , param.Find(x => x.ParameterName == "@OrderId")
                        , param.Find(x => x.ParameterName == "@ProductId")
                        , param.Find(x => x.ParameterName == "@ProductTemplateId")
                        , param.Find(x => x.ParameterName == "@FabricMaterialId")
                        , param.Find(x => x.ParameterName == "@MaterialValue")
                        , param.Find(x => x.ParameterName == "@IsCusMaterial")
                        , param.Find(x => x.ParameterName == "@ProductName")
                        , param.Find(x => x.ParameterName == "@SaveOrderComponents")
                        , param.Find(x => x.ParameterName == "@Note")
                        , param.Find(x => x.ParameterName == "@ProfileBodyId"));
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    var result = await productRepository.GetStoreProcedureReturnInt("DeleteProduct", param.Find(x => x.ParameterName == "@ProductId"));

                    throw new UserException(ex.Message);
                }
            }

            if (results.All(x => x == 1))
            {
                await orderService.CheckOrderPaid(orderId);

                return Ultils.GenGuidString();
            }
            else
            {
                throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình tạo sản phẩm", nameof(ProductService));
            }
        }

        public async Task<string> UpdateProduct(string wwwroot, string orderId, Product product, List<ProductComponent> productComponents, string materialId, string profileId, bool isCusMaterial, double materialQuantity)
        {
            //throw new UserException("Nhắc tk Tùng sửa cái proc UpdateProduct lại");

            var dbProduct = productRepository.Get(product.Id);
            if (dbProduct == null)
            {
                throw new UserException("Không tìm thấy sản phẩm");
            }

            try
            {
                #region GetData
                var templateId = new SqlParameter("@ProductTemplateId", System.Data.SqlDbType.NVarChar)
                {
                    Value = product.ProductTemplateId != null ? product.ProductTemplateId : DBNull.Value
                };

                //các bộ phận của bản mẫu
                var templateComponentTypes = componentTypeRepository.GetStoreProcedure(StoreProcName.Get_Template_Component_Types, templateId);
                if (templateComponentTypes != null && templateComponentTypes.Any())
                {
                    templateComponentTypes = templateComponentTypes.ToList();
                }

                //các kiểu bộ phận của bản mẫu
                var templateComponents = componentRepository.GetStoreProcedure(StoreProcName.Get_Template_Components, templateId);
                if (templateComponents != null && templateComponents.Any())
                {
                    templateComponents = templateComponents.ToList();
                }
                #endregion

                if (!string.IsNullOrEmpty(dbProduct.SaveOrderComponents))
                {
                    var newSaveOrderComponents = new List<ProductComponent>();
                    var saveOrderComponents = JsonConvert.DeserializeObject<List<ProductComponent>>(dbProduct.SaveOrderComponents);

                    if (saveOrderComponents != null && saveOrderComponents.Any())
                    {
                        if (templateComponentTypes != null && templateComponentTypes.Any())
                        {
                            var insideTasks = new List<Task>();
                            foreach (var templateComponentType in templateComponentTypes)
                            {
                                insideTasks.Add(Task.Run(async () =>
                                {
                                    var components = templateComponents.Where(x => x.ComponentTypeId == templateComponentType.Id);
                                    if (components != null)
                                    {
                                        var saveOrderComponent = saveOrderComponents.FirstOrDefault(x => components.Select(c => c.Id).Contains(x.ComponentId));
                                        var newOrderComponent = productComponents.FirstOrDefault(x => components.Select(c => c.Id).Contains(x.ComponentId));
                                        if (saveOrderComponent == null && newOrderComponent == null)
                                        {
                                            var defaultComponent = components.FirstOrDefault(x => x.Default == true);

                                            if (defaultComponent == null)
                                            {
                                                defaultComponent = components.First();
                                            }

                                            newSaveOrderComponents.Add(new ProductComponent()
                                            {
                                                Id = Ultils.GenGuidString(),
                                                ComponentId = defaultComponent.Id,
                                                Name = defaultComponent.Name,
                                                Image = "",
                                                ProductStageId = null,
                                                LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                                Note = null,
                                                NoteImage = null
                                            });
                                        }
                                        else if (saveOrderComponent != null && newOrderComponent == null)
                                        {
                                            newSaveOrderComponents.Add(saveOrderComponent);
                                        }
                                        else if (saveOrderComponent == null && newOrderComponent != null)
                                        {
                                            var selectedComponent = components.FirstOrDefault(x => x.Id == newOrderComponent.ComponentId);
                                            if (selectedComponent != null)
                                            {
                                                if (!string.IsNullOrEmpty(newOrderComponent.NoteImage))
                                                {
                                                    var listStringImage = JsonConvert.DeserializeObject<List<string>>(newOrderComponent.NoteImage);
                                                    var listImage = new List<string>();
                                                    if (listStringImage != null && listStringImage.Count > 0)
                                                    {
                                                        var insideTask1s = new List<Task>();
                                                        foreach (var item in listStringImage)
                                                        {
                                                            insideTask1s.Add(Task.Run(async () =>
                                                            {
                                                                var image = JsonConvert.DeserializeObject<FileDTO>(item);
                                                                listImage.Add(await Ultils.UploadImageBase64(wwwroot, $"Product/{product.Id}/Component/{selectedComponent.Id}", image.Base64String, image.FileName, image.ContentType, null));
                                                            }));
                                                        }
                                                        await Task.WhenAll(insideTask1s);

                                                        newOrderComponent.NoteImage = JsonConvert.SerializeObject(listImage);
                                                    }
                                                }
                                                newSaveOrderComponents.Add(new ProductComponent()
                                                {
                                                    Id = Ultils.GenGuidString(),
                                                    ComponentId = selectedComponent.Id,
                                                    Name = selectedComponent.Name,
                                                    Image = "",
                                                    ProductStageId = null,
                                                    LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                                    Note = newOrderComponent.Note,
                                                    NoteImage = newOrderComponent.NoteImage
                                                });
                                            }
                                            else
                                            {
                                                throw new UserException($"Không tìm thấy kiểu bộ phận: {templateComponentType.Name}");
                                            }
                                        }
                                        else if (saveOrderComponent != null && newOrderComponent != null)
                                        {
                                            var selectedComponent = components.FirstOrDefault(x => x.Id == newOrderComponent.ComponentId);
                                            if (selectedComponent != null)
                                            {
                                                if (saveOrderComponent.ComponentId != selectedComponent.Id)
                                                {
                                                    saveOrderComponent.ComponentId = selectedComponent.Id;
                                                    saveOrderComponent.Name = selectedComponent.Name;
                                                }
                                                saveOrderComponent.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                                saveOrderComponent.Note = newOrderComponent.Note;

                                                if (!string.IsNullOrEmpty(newOrderComponent.NoteImage))
                                                {
                                                    var listImage = new List<string>();

                                                    var listStringImage = JsonConvert.DeserializeObject<List<string>>(newOrderComponent.NoteImage);
                                                    if (listStringImage != null && listStringImage.Any())
                                                    {
                                                        var listOldImageUrl = listStringImage.Where(x => x.Contains("http")).ToList();
                                                        if (listOldImageUrl != null && listOldImageUrl.Count > 0)
                                                        {
                                                            if (!string.IsNullOrEmpty(saveOrderComponent.NoteImage))
                                                            {
                                                                var listOldImage = JsonConvert.DeserializeObject<List<string>>(saveOrderComponent.NoteImage);
                                                                if (listOldImage != null && listOldImage.Count > 0)
                                                                {
                                                                    var checkOldImageTasks = new List<Task>();
                                                                    foreach (var item in listOldImage)
                                                                    {
                                                                        checkOldImageTasks.Add(Task.Run(() =>
                                                                        {
                                                                            var imageObject = JsonConvert.DeserializeObject<ImageFileDTO>(item);
                                                                            if (listOldImageUrl.Contains(imageObject.ObjectUrl))
                                                                            {
                                                                                listImage.Add(item);
                                                                            }
                                                                            else
                                                                            {
                                                                                Ultils.DeleteObject(imageObject.ObjectName);
                                                                            }
                                                                        }));
                                                                    }
                                                                    await Task.WhenAll(checkOldImageTasks);
                                                                }
                                                            }
                                                        }

                                                        var listNewImageFile = listStringImage.Where(x => !x.Contains("http")).ToList();
                                                        if (listNewImageFile != null && listNewImageFile.Count > 0)
                                                        {
                                                            var insideTask1s = new List<Task>();
                                                            foreach (var item in listNewImageFile)
                                                            {
                                                                insideTask1s.Add(Task.Run(async () =>
                                                                {
                                                                    var image = JsonConvert.DeserializeObject<FileDTO>(item);
                                                                    listImage.Add(await Ultils.UploadImageBase64(
                                                                            wwwroot,
                                                                            $"Product/{product.Id}/Component/{selectedComponent.Id}",
                                                                            image.Base64String,
                                                                            image.FileName,
                                                                            image.ContentType,
                                                                            null
                                                                        )
                                                                    );
                                                                }));
                                                            }
                                                            await Task.WhenAll(insideTask1s);
                                                        }
                                                    }
                                                    if (listImage.Count > 0)
                                                    {
                                                        saveOrderComponent.NoteImage = JsonConvert.SerializeObject(listImage);
                                                    }
                                                    else
                                                    {
                                                        saveOrderComponent.NoteImage = null;
                                                    }
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(saveOrderComponent.NoteImage))
                                                    {
                                                        var listOldImage = JsonConvert.DeserializeObject<List<string>>(saveOrderComponent.NoteImage);
                                                        if (listOldImage != null && listOldImage.Count > 0)
                                                        {
                                                            var deleteOldImageTasks = new List<Task>();
                                                            foreach (var item in listOldImage)
                                                            {
                                                                deleteOldImageTasks.Add(Task.Run(() =>
                                                                {
                                                                    var imageObject = JsonConvert.DeserializeObject<ImageFileDTO>(item);
                                                                    Ultils.DeleteObject(imageObject.ObjectName);
                                                                }));
                                                            }

                                                            await Task.WhenAll(deleteOldImageTasks);
                                                        }
                                                    }
                                                    saveOrderComponent.NoteImage = null;
                                                }
                                            }
                                            else
                                            {
                                                throw new UserException($"Không tìm thấy kiểu bộ phận: {templateComponentType.Name}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        throw new UserException($"Không tìm thấy kiểu bộ phận: {templateComponentType.Name}");
                                    }
                                }));
                            }

                            await Task.WhenAll(insideTasks);
                        }
                        else
                        {
                            throw new UserException("Không tìm thấy kiểu bộ phận");
                        }

                        product.SaveOrderComponents = JsonConvert.SerializeObject(saveOrderComponents);
                    }
                    else
                    {
                        saveOrderComponents = new List<ProductComponent>();

                        if (templateComponentTypes != null && templateComponentTypes.Any())
                        {
                            do
                            {
                                saveOrderComponents = new List<ProductComponent>();
                                var insideTasks = new List<Task>();
                                foreach (var type in templateComponentTypes)
                                {
                                    insideTasks.Add(Task.Run(async () =>
                                        {
                                            var componentIds = string.Join(",", productComponents.Select(c => c.ComponentId));
                                            var productComponentAdds = templateComponents.Where(x => x.ComponentTypeId == type.Id && componentIds.Contains(x.Id));
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
                                                component = templateComponents.FirstOrDefault(x => x.ComponentTypeId == type.Id && x.Default == true);
                                            }

                                            if (component != null)
                                            {
                                                var productComponent = productComponents.FirstOrDefault(x => x.ComponentId == component.Id);
                                                if (productComponent != null && !string.IsNullOrEmpty(productComponent.NoteImage))
                                                {
                                                    var listStringImage = JsonConvert.DeserializeObject<List<string>>(productComponent.NoteImage);
                                                    var listImage = new List<string>();
                                                    if (listStringImage != null && listStringImage.Count > 0)
                                                    {
                                                        var insideTask1s = new List<Task>();
                                                        foreach (var item in listStringImage)
                                                        {
                                                            insideTask1s.Add(Task.Run(async () =>
                                                            {
                                                                var image = JsonConvert.DeserializeObject<FileDTO>(item);
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
                                                            }));
                                                        }
                                                        await Task.WhenAll(insideTask1s);

                                                        productComponent.NoteImage = JsonConvert.SerializeObject(listImage);
                                                    }
                                                }
                                                saveOrderComponents.Add(new ProductComponent()
                                                {
                                                    ComponentId = component.Id,
                                                    Id = Ultils.GenGuidString(),
                                                    LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                                    Name = component.Name,
                                                    Image = "",
                                                    ProductStageId = null,
                                                    Note = productComponent?.Note,
                                                    NoteImage = productComponent?.NoteImage
                                                });
                                            }
                                            else
                                            {
                                                throw new UserException($"Không tìm thấy kiểu bộ phận: {type.Name}");
                                            }
                                        })
                                    );
                                }
                                await Task.WhenAll(insideTasks);
                            } while (saveOrderComponents.Count != templateComponentTypes.Count());
                        }
                        else
                        {
                            throw new UserException("Không tìm thấy kiểu bộ phận");
                        }

                        product.SaveOrderComponents = JsonConvert.SerializeObject(saveOrderComponents);
                    }
                }

                #region SetupProcParam
                var orderIdParam = new SqlParameter("@OrderId", System.Data.SqlDbType.NVarChar)
                {
                    Value = orderId
                };
                var productId = new SqlParameter("@ProductId", System.Data.SqlDbType.NVarChar)
                {
                    Value = product.Id
                };
                var productTemplateId = new SqlParameter("@ProductTemplateId", System.Data.SqlDbType.NVarChar)
                {
                    Value = product.ProductTemplateId != null ? product.ProductTemplateId : DBNull.Value
                };
                var fabricMaterialId = new SqlParameter("@NewFabricMaterialId", System.Data.SqlDbType.NVarChar)
                {
                    Value = materialId
                };
                var materialValue = new SqlParameter("@MaterialValue", System.Data.SqlDbType.Decimal)
                {
                    Value = (materialQuantity != null || materialQuantity > 0) ? materialQuantity : 0
                };
                var isCusMaterialParam = new SqlParameter("@IsCusMaterial", System.Data.SqlDbType.Bit)
                {
                    Value = isCusMaterial ? 1 : 0
                };
                var productName = new SqlParameter("@NewProductName", System.Data.SqlDbType.NVarChar)
                {
                    Value = !string.IsNullOrWhiteSpace(product.Name) ? product.Name : DBNull.Value
                };
                var note = new SqlParameter("@NewNote", System.Data.SqlDbType.NVarChar)
                {
                    Value = !string.IsNullOrWhiteSpace(product.Note) ? product.Note : DBNull.Value
                };
                var saveOrderComponentsParam = new SqlParameter("@NewSaveOrderComponents", System.Data.SqlDbType.NVarChar)
                {
                    Value = !string.IsNullOrWhiteSpace(product.SaveOrderComponents) ? product.SaveOrderComponents : DBNull.Value
                };
                var profileBodyId = new SqlParameter("@NewProfileBodyId", System.Data.SqlDbType.NVarChar)
                {
                    Value = profileId
                };

                #endregion
                try
                {
                    var result = await productRepository.GetStoreProcedureReturnInt("UpdateProduct"
                        , orderIdParam, productId, productTemplateId, fabricMaterialId, materialValue, isCusMaterialParam, productName, saveOrderComponentsParam, note, profileBodyId);

                    if (result == 1)
                    {
                        await orderService.CheckOrderPaid(orderId);

                        return product.Id;
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật sản phẩm", nameof(ProductService.UpdateProduct));
                    }
                }
                catch (Exception ex)
                {
                    throw new UserException(ex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }

        public async Task<bool> UpdateProductPrice(string orderId, string productId, decimal? price)
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

                                if (productRepository.Update(dbProduct.Id, dbProduct))
                                {
                                    return await orderService.CheckOrderPaid(dbProduct.OrderId);
                                }
                                else
                                {
                                    throw new SystemsException($"Error in {nameof(ProductService)}: Lỗi trong quá trình cập nhật giá sản phẩm", nameof(ProductService.UpdateProductPrice));
                                }
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
                            var sameProductMaterialdbs = productRepository.GetAll(x => x.OrderId == order.Id && x.FabricMaterialId == dbProduct.FabricMaterialId && x.IsActive == true && x.Status > 0);
                            if (sameProductMaterialdbs != null && sameProductMaterialdbs.Any())
                            {
                                sameProductMaterialdbs = null;
                            }
                            else
                            {
                                sameProductMaterialdbs = null;
                                var orderMaterial = orderMaterialRepository.GetAll(x => x.OrderId == order.Id && x.MaterialId == dbProduct.FabricMaterialId && x.IsActive == true);
                                if (orderMaterial != null && orderMaterial.Any())
                                {
                                    orderMaterial = orderMaterial.ToList();
                                    if (orderMaterial.Count() > 1)
                                    {
                                        throw new SystemsException("Lỗi trong quá trình xóa order materials", nameof(ProductService));
                                    }
                                    else
                                    {
                                        if (!orderMaterialRepository.Delete(orderMaterial.First().Id))
                                        {
                                            throw new SystemsException("Lỗi trong quá trình xóa order materials", nameof(ProductService));
                                        }
                                    }
                                }
                            }
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

        public async Task<IEnumerable<Product>> GetProductsByOrderIdOfCus(string orderId, string cusId)
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

        public async Task<List<ProductBodySize>> GetBodySizeOfProduct(string productId, string orderId, string? cusId)
        {
            var dbOrder = orderRepository.Get(orderId);
            if (dbOrder != null && dbOrder.Status > 0)
            {
                if (!string.IsNullOrEmpty(cusId) && dbOrder.CustomerId != cusId)
                {
                    throw new UserException("Không tìm thấy hóa đơn");
                }

                var dbProduct = productRepository.Get(productId);
                if (dbProduct != null && dbProduct.IsActive == true && dbProduct.Status > 0)
                {
                    var productBodySizes = productBodySizeRepository.GetAll(x => x.ProductId == productId && x.IsActive == true);

                    if (productBodySizes != null && productBodySizes.Any())
                    {
                        productBodySizes = productBodySizes.ToList();

                        var bodySizeIds = string.Join(",", productBodySizes.Select(x => x.BodySizeId));
                        var bodySizes = bodySizeRepository.GetAll(x => bodySizeIds.Contains(x.Id));
                        if (bodySizes != null && bodySizes.Any())
                        {
                            bodySizes = bodySizes.ToList();

                            var tasks = new List<Task>();
                            var listProductBodySizes = new List<ProductBodySize>();
                            foreach (var bodySize in bodySizes)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    var productBodySize = productBodySizes.FirstOrDefault(x => x.BodySizeId == bodySize.Id);
                                    if (productBodySize != null)
                                    {
                                        productBodySize.BodySize = bodySize;
                                        listProductBodySizes.Add(productBodySize);
                                    }
                                    else
                                    {
                                        listProductBodySizes.Add(new ProductBodySize()
                                        {
                                            Id = Ultils.GenGuidString(),
                                            ProductId = productId,
                                            BodySizeId = bodySize.Id,
                                            IsActive = true,
                                            CreatedTime = DateTime.UtcNow.AddHours(7),
                                            LastestUpdatedTime = DateTime.UtcNow.AddHours(7),
                                            BodySize = bodySize,
                                            Value = 0,
                                            InactiveTime = null
                                        });
                                    }
                                }));
                            }

                            await Task.WhenAll(tasks);
                            return listProductBodySizes;
                        }
                        else
                        {
                            throw new UserException("Không tìm thấy số đo hệ thống");
                        }
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy số đo sản phẩm");
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy sản phẩm");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public async Task<IEnumerable<ComponentType>> GetProductComponent(string templateId)
        {
            var template = productTemplateRepository.Get(templateId);
            if (template == null)
            {
                throw new UserException("Bản mẫu không tìm thấy");
            }
            else
            {

                var componentTypes = componentTypeRepository.GetAll(x => x.CategoryId == template.CategoryId && x.IsActive == true);
                if (componentTypes != null && componentTypes.Any())
                {
                    componentTypes = componentTypes.ToList();

                    return componentTypes;
                }
                return null;
            }
        }
    }
}
