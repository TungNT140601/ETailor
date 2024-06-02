using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class TaskService : ITaskService
    {
        private readonly IProductRepository productRepository;
        private readonly IProductStageRepository productStageRepository;
        private readonly IProductComponentRepository productComponentRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IBodySizeRepository bodySizeRepository;
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        private readonly IMaterialRepository materialRepository;
        private readonly IMaterialTypeRepository materialTypeRepository;
        private readonly ITemplateStateRepository templateStateRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IComponentStageRepository componentStageRepository;
        private readonly IProfileBodyRepository profileBodyRepository;
        private readonly IOrderMaterialRepository orderMaterialRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IMasteryRepository masteryRepository;
        private readonly IProductBodySizeRepository productBodySizeRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IProductStageMaterialRepository productStageMaterialRepository;
        private readonly ISignalRService signalRService;
        private readonly INotificationService notificationService;
        private readonly INotificationRepository notificationRepository;

        public TaskService(IProductRepository productRepository, IProductStageRepository productStageRepository
            , IProductComponentRepository productComponentRepository, IOrderRepository orderRepository
            , IProductTemplateRepository productTemplateRepository, IComponentRepository componentRepository
            , IProductBodySizeRepository productBodySizeRepository, IBodySizeRepository bodySizeRepository
            , IMaterialCategoryRepository materialCategoryRepository, IMaterialRepository materialRepository
            , IMaterialTypeRepository materialTypeRepository, ITemplateStateRepository templateStateRepository
            , IComponentTypeRepository componentTypeRepository, IComponentStageRepository componentStageRepository
            , IProfileBodyRepository profileBodyRepository, IOrderMaterialRepository orderMaterialRepository
            , IStaffRepository staffRepository, IMasteryRepository masteryRepository, ICategoryRepository categoryRepository
            , IProductStageMaterialRepository productStageMaterialRepository, ISignalRService signalRService
            , INotificationService notificationService, INotificationRepository notificationRepository)
        {
            this.productRepository = productRepository;
            this.productStageRepository = productStageRepository;
            this.productComponentRepository = productComponentRepository;
            this.orderRepository = orderRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.componentRepository = componentRepository;
            this.productBodySizeRepository = productBodySizeRepository;
            this.bodySizeRepository = bodySizeRepository;
            this.materialCategoryRepository = materialCategoryRepository;
            this.materialTypeRepository = materialTypeRepository;
            this.materialRepository = materialRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.templateStateRepository = templateStateRepository;
            this.componentStageRepository = componentStageRepository;
            this.profileBodyRepository = profileBodyRepository;
            this.orderMaterialRepository = orderMaterialRepository;
            this.staffRepository = staffRepository;
            this.masteryRepository = masteryRepository;
            this.categoryRepository = categoryRepository;
            this.productStageMaterialRepository = productStageMaterialRepository;
            this.signalRService = signalRService;
            this.notificationService = notificationService;
            this.notificationRepository = notificationRepository;
        }

        public async Task<Product> GetTask(string productId)
        {
            var tasks = new List<Task>();

            var product = productRepository.Get(productId);

            if (product != null && product.Status > 0 && product.IsActive == true)
            {
                var order = orderRepository.Get(product.OrderId);
                var fabricMaterial = materialRepository.Get(product.FabricMaterialId);
                var template = productTemplateRepository.Get(product.ProductTemplateId);
                var productStages = productStageRepository.GetAll(x => x.ProductId == productId && x.IsActive == true);
                var productBodySizes = productBodySizeRepository.GetAll(x => product != null && x.ProductId == productId);

                if (!(order != null && order.Status >= 1 && order.IsActive == true))
                {
                    return null;
                }
                tasks.Add(Task.Run(() =>
                {
                    if (product.Order == null)
                    {
                        product.Order = order;
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (product.FabricMaterial == null)
                    {
                        product.FabricMaterial = fabricMaterial;
                    }
                    if (!string.IsNullOrEmpty(product.FabricMaterial.Image) && !product.FabricMaterial.Image.StartsWith("https://firebasestorage.googleapis.com"))
                    {
                        product.FabricMaterial.Image = Ultils.GetUrlImage(product.FabricMaterial.Image);
                    }
                    else if (product.FabricMaterial.Image.StartsWith("https://firebasestorage.googleapis.com/"))
                    {

                    }
                    else
                    {
                        product.FabricMaterial.Image = string.Empty;
                    }
                }));
                tasks.Add(Task.Run(() =>
                {
                    if (product.ProductTemplate == null)
                    {
                        product.ProductTemplate = template;
                    }
                    if (!string.IsNullOrEmpty(product.ProductTemplate.ThumbnailImage) && !product.ProductTemplate.ThumbnailImage.StartsWith("https://firebasestorage.googleapis.com/"))
                    {
                        product.ProductTemplate.ThumbnailImage = Ultils.GetUrlImage(product.ProductTemplate.ThumbnailImage);
                    }
                    else if (product.ProductTemplate.Image.StartsWith("https://firebasestorage.googleapis.com/"))
                    {

                    }
                    else
                    {
                        product.ProductTemplate.ThumbnailImage = string.Empty;
                    }
                }));

                if (template != null)
                {
                    if (productStages != null && productStages.Any())
                    {
                        productStages = productStages.OrderBy(x => x.StageNum).ToList();

                        var productComponents = productComponentRepository.GetAll(x => productStages.Select(c => c.Id).Contains(x.ProductStageId));

                        if (productComponents != null && productComponents.Any())
                        {
                            productComponents = productComponents.ToList();

                            var productStageMaterials = productStageMaterialRepository.GetAll(x => productStages.Select(c => c.Id).Contains(x.ProductStageId));

                            var materialStages = new List<Material>();

                            if (productStageMaterials != null && productStageMaterials.Any())
                            {
                                productStageMaterials = productStageMaterials.ToList();

                                materialStages = materialRepository.GetAll(x => productStageMaterials.Select(c => c.MaterialId).Contains(x.Id))?.ToList();

                                if (materialStages != null && materialStages.Any())
                                {
                                    materialStages = materialStages.ToList();
                                }
                                else
                                {
                                    materialStages = new List<Material>();
                                }
                            }
                            else
                            {
                                productStageMaterials = new List<ProductStageMaterial>();
                            }

                            var productTemplateComponents = componentRepository.GetAll(x => productComponents != null && productComponents.Any() && productComponents.Select(c => c.ComponentId).Contains(x.Id));

                            if (productTemplateComponents != null && productTemplateComponents.Any())
                            {
                                productTemplateComponents = productTemplateComponents.ToList();

                                await Task.WhenAll(tasks);

                                tasks.Add(Task.Run(() =>
                                {
                                    if (product.ProductStages == null)
                                    {
                                        product.ProductStages = productStages.ToList();
                                    }
                                }));

                                tasks.Add(Task.Run(async () =>
                                {
                                    var insideTasks1 = new List<Task>();
                                    foreach (var stage in product.ProductStages)
                                    {
                                        insideTasks1.Add(Task.Run(async () =>
                                        {
                                            var insideTasks2 = new List<Task>();

                                            insideTasks2.Add(Task.Run(async () =>
                                            {
                                                stage.ProductStageMaterials = productStageMaterials?.Where(x => x.ProductStageId == stage.Id)?.ToList();

                                                if (stage.ProductStageMaterials != null && stage.ProductStageMaterials.Any())
                                                {
                                                    var insideTasks3 = new List<Task>();
                                                    foreach (var stageMaterial in stage.ProductStageMaterials)
                                                    {
                                                        insideTasks3.Add(Task.Run(() =>
                                                        {
                                                            stageMaterial.Material = materialStages.FirstOrDefault(x => x.Id == stageMaterial.MaterialId);
                                                            if (stageMaterial.Material != null && !string.IsNullOrEmpty(stageMaterial.Material.Image))
                                                            {
                                                                stageMaterial.Material.Image = Ultils.GetUrlImage(stageMaterial.Material.Image);
                                                            }
                                                        }));
                                                    }
                                                    await Task.WhenAll(insideTasks3);
                                                }
                                                else
                                                {
                                                    stage.ProductStageMaterials = null;
                                                }
                                            }));

                                            insideTasks2.Add(Task.Run(async () =>
                                            {
                                                if (stage.ProductComponents == null || !stage.ProductComponents.Any())
                                                {
                                                    stage.ProductComponents = productComponents.Where(x => x.ProductStageId == stage.Id).ToList();
                                                }

                                                var insideTasks3 = new List<Task>();

                                                foreach (var component in stage.ProductComponents)
                                                {
                                                    insideTasks3.Add(Task.Run(async () =>
                                                    {
                                                        if (component.Component == null)
                                                        {
                                                            component.Component = productTemplateComponents.FirstOrDefault(x => x.Id == component.ComponentId);
                                                        }
                                                        if (!string.IsNullOrEmpty(component.Component.Image) && !component.Component.Image.StartsWith("https://firebasestorage.googleapis.com/"))
                                                        {
                                                            component.Component.Image = Ultils.GetUrlImage(component.Component.Image);
                                                        }
                                                        else if (component.Component.Image.StartsWith("https://firebasestorage.googleapis.com/"))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            component.Component.Image = string.Empty;
                                                        }

                                                        if (!string.IsNullOrWhiteSpace(component.NoteImage))
                                                        {
                                                            var noteImage = JsonConvert.DeserializeObject<List<string>>(component.NoteImage);
                                                            if (noteImage != null && noteImage.Any())
                                                            {
                                                                var noteImages = new List<string>();
                                                                var insideTasks4 = new List<Task>();
                                                                foreach (var image in noteImage)
                                                                {
                                                                    insideTasks4.Add(Task.Run(async () =>
                                                                    {
                                                                        if (!image.StartsWith("https://firebasestorage.googleapis.com/"))
                                                                        {
                                                                            noteImages.Add(Ultils.GetUrlImage(image));
                                                                        }
                                                                        else
                                                                        {
                                                                            noteImages.Add(image);
                                                                        }
                                                                    }));
                                                                }
                                                                await Task.WhenAll(insideTasks4);
                                                                component.NoteImage = JsonConvert.SerializeObject(noteImages);
                                                            }
                                                        }
                                                    }));
                                                }

                                                await Task.WhenAll(insideTasks3);
                                            }));

                                            await Task.WhenAll(insideTasks2);
                                        }));
                                    }
                                    await Task.WhenAll(insideTasks1);
                                }));
                            }
                        }
                    }

                    if (productBodySizes != null && productBodySizes.Any())
                    {
                        productBodySizes = productBodySizes.ToList();

                        var bodySizes = bodySizeRepository.GetAll(x => productBodySizes != null && productBodySizes.Any() && productBodySizes.Select(c => c.BodySizeId).Contains(x.Id));

                        if (bodySizes != null && bodySizes.Any())
                        {
                            bodySizes = bodySizes.ToList();

                            tasks.Add(Task.Run(async () =>
                            {
                                var insideTasks1 = new List<Task>();
                                insideTasks1.Add(Task.Run(() =>
                                {
                                    if (product.ProductBodySizes == null)
                                    {
                                        product.ProductBodySizes = productBodySizes.ToList();
                                    }
                                }));
                                insideTasks1.Add(Task.Run(async () =>
                                {
                                    var insideTasks2 = new List<Task>();
                                    foreach (var productBodySize in product.ProductBodySizes)
                                    {
                                        insideTasks2.Add(Task.Run(() =>
                                        {
                                            if (productBodySize.BodySize == null)
                                            {
                                                productBodySize.BodySize = bodySizes.FirstOrDefault(x => x.Id == productBodySize.BodySizeId);
                                            }
                                        }));
                                    }
                                    await Task.WhenAll(insideTasks2);
                                }));
                                await Task.WhenAll(insideTasks1);
                            }));

                            await Task.WhenAll(tasks);

                            product.ProductStages = product.ProductStages.OrderBy(x => x.StageNum).ToList();
                        }
                    }
                }

                return product;
            }

            return null;
        }

        public async Task<IEnumerable<ProductStage>> GetProductStagesOfEachTask(string taskId)
        {
            IEnumerable<ProductStage> ListOfProductStage = productStageRepository.GetAll(x => (taskId != null && x.ProductId == taskId) && x.IsActive == true).OrderBy(x => x.StageNum);
            return ListOfProductStage;
        }

        public async Task<IEnumerable<Product>> GetTasks()
        {
            var inProcessOrders = orderRepository.GetAll(x => x.Status >= 2 && x.Status < 8 && x.IsActive == true);
            if (inProcessOrders != null && inProcessOrders.Any())
            {
                inProcessOrders = inProcessOrders.ToList();

                var tasksList = productRepository.GetAll(x => inProcessOrders.Select(c => c.Id).Contains(x.OrderId) && x.Status > 0 && x.Status < 5 && x.IsActive == true);

                if (tasksList != null && tasksList.Any())
                {
                    tasksList = tasksList.OrderBy(x => x.Index).ToList();

                    var staffs = staffRepository.GetAll(x => true);
                    if (staffs != null && staffs.Any())
                    {
                        staffs = staffs.ToList();

                        var tasks = new List<Task>();
                        foreach (var task in tasksList)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                if (task.StaffMakerId != null)
                                {
                                    if (task.StaffMaker == null)
                                    {
                                        task.StaffMaker = staffs.FirstOrDefault(x => x.Id == task.StaffMakerId);
                                    }
                                    if (task.StaffMaker != null)
                                    {
                                        if (!string.IsNullOrEmpty(task.StaffMaker.Avatar))
                                        {
                                            task.StaffMaker.Avatar = Ultils.GetUrlImage(task.StaffMaker.Avatar);
                                        }
                                    }
                                }
                            }));
                        }

                        await Task.WhenAll(tasks);
                    }
                }

                return tasksList;
            }

            return null;
        }

        public async Task<IEnumerable<Product>> GetTasksByStaffId(string staffId)
        {
            return productRepository.GetAll(x => x.StaffMakerId == staffId && x.Status > 0 && x.IsActive == true);
        }

        public async Task AutoCreateEmptyTaskProduct()
        {
            try
            {
                var startTime = DateTime.UtcNow.AddHours(7);

                var approveOrders = orderRepository.GetAll(x => (x.Status == 2 || x.Status == 3) && x.IsActive == true);
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
                                            productAllDbs = productAllDbs.ToList();
                                            greatestIndexDb = productAllDbs.OrderByDescending(x => x.Index).FirstOrDefault()?.Index;
                                        }
                                        else
                                        {
                                            productAllDbs = null;
                                        }

                                        if (greatestIndexDb == null)
                                        {
                                            greatestIndexDb = 1;
                                        }
                                        else
                                        {
                                            greatestIndexDb++;
                                        }


                                        var existProductStages = productStageRepository.GetAll(x => approveOrdersProducts.Select(c => c.Id).Contains(x.ProductId) && x.IsActive == true);

                                        if (existProductStages != null && existProductStages.Any())
                                        {
                                            existProductStages = existProductStages.ToList();
                                        }
                                        else
                                        {
                                            existProductStages = null;
                                        }

                                        var check = new List<bool>();

                                        var orderTasks = new List<Task>();
                                        foreach (var order in approveOrders)
                                        {
                                            orderTasks.Add(Task.Run(async () =>
                                            {
                                                if (approveOrdersProducts.Any(x => x.OrderId == order.Id))
                                                {
                                                    order.Products = approveOrdersProducts.Where(x => x.OrderId == order.Id).ToList();
                                                    var productTasks = new List<Task>();
                                                    foreach (var product in order.Products)
                                                    {
                                                        productTasks.Add(Task.Run(async () =>
                                                        {
                                                            var dbProductStages = existProductStages?.Where(x => x.ProductId == product.Id);

                                                            if (dbProductStages == null || !dbProductStages.Any())
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

                                                                            var stageTasks = new List<Task>();

                                                                            foreach (var stage in productTemplateStagees)
                                                                            {
                                                                                stageTasks.Add(Task.Run(async () =>
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
                                                                                    StageName = stage.Name,
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
                                                                                            var productComponentsInStage = productComponents.Where(x => componentsInStage.Select(c => c.Id).Contains(x.ComponentId));
                                                                                            if (productComponentsInStage != null && productComponentsInStage.Any())
                                                                                            {
                                                                                                productComponentsInStage = productComponentsInStage.ToList();
                                                                                                var productComponentTasks = new List<Task>();
                                                                                                foreach (var productComponent in productComponentsInStage)
                                                                                                {
                                                                                                    productComponentTasks.Add(Task.Run(() =>
                                                                                                    {
                                                                                                        productComponent.Id = Ultils.GenGuidString();
                                                                                                        productComponent.LastestUpdatedTime = DateTime.Now;
                                                                                                        productComponent.Name = components.FirstOrDefault(x => x.Id == productComponent.ComponentId)?.Name;
                                                                                                        productComponent.ProductStageId = productStage.Id;

                                                                                                        productStage.ProductComponents.Add(productComponent);
                                                                                                    }));
                                                                                                }
                                                                                                await Task.WhenAll(productComponentTasks);
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }

                                                                                product.ProductStages.Add(productStage);
                                                                            }));
                                                                            }
                                                                            await Task.WhenAll(stageTasks);
                                                                        }
                                                                    }
                                                                }
                                                                greatestIndexDb++;
                                                            }
                                                        }));
                                                    }
                                                    await Task.WhenAll(productTasks);
                                                }
                                                order.Status = 3;
                                            }));
                                        }
                                        await Task.WhenAll(orderTasks);
                                        if (!orderRepository.UpdateRange(approveOrders.ToList()))
                                        {
                                            throw new SystemsException($"Error in {nameof(TaskService.AutoCreateEmptyTaskProduct)}: Lỗi trong quá trình tự động tạo task", nameof(TaskService.AutoCreateEmptyTaskProduct));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var endTime = DateTime.UtcNow.AddHours(7);
            }
            catch (Exception ex)
            {
                throw new SystemsException($"Error in {nameof(TaskService.AutoCreateEmptyTaskProduct)}: {ex.Message}", nameof(TaskService.AutoCreateEmptyTaskProduct));
            }
        }

        public void AutoAssignTaskForStaff()
        {
            try
            {
                var startTime = DateTime.UtcNow.AddHours(7);
                var checkException = false;
                var notStartOrders = orderRepository.GetAll(x => x.Status == 3 && x.IsActive == true);
                if (notStartOrders != null && notStartOrders.Any())
                {
                    notStartOrders = notStartOrders.OrderBy(x => x.CreatedTime).ToList();

                    var notStartEmptyTaskProducts = productRepository.GetAll(x => notStartOrders.Select(c => c.Id).Contains(x.OrderId) && x.StaffMakerId == null && x.Status > 0 && x.Status < 8 && x.IsActive == true);
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
                                                        var staffCurrentTasks = productRepository.GetAll(x => x.StaffMakerId == staff.Id && x.Status > 0 && x.Status < 5 && x.IsActive == true);
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

                                                    if (product.ProductStages == null || !product.ProductStages.Any())
                                                    {
                                                        product.ProductStages = productStageRepository.GetAll(x => x.ProductId == product.Id && x.IsActive == true).ToList();
                                                        foreach (var stage in product.ProductStages)
                                                        {
                                                            stage.Staff = null;
                                                            stage.StaffId = staffId;
                                                            stage.TaskIndex = stage.StageNum;
                                                        }
                                                    }
                                                    try
                                                    {
                                                        if (!productRepository.Update(product.Id, product))
                                                        {
                                                            throw new SystemsException($"Error in {nameof(TaskService.AutoAssignTaskForStaff)}: Lỗi trong quá trình tự động phân công công việc cho nhân viên", nameof(TaskService.AutoAssignTaskForStaff));
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
                    throw new SystemsException("Lỗi trong quá trình tự động phân công công việc cho nhân viên", nameof(TaskService.AutoAssignTaskForStaff));
                }

                var endTime = DateTime.UtcNow.AddHours(7);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(TaskService.AutoAssignTaskForStaff));
            }
        }

        public async Task<bool> SetDeadlineForTask(string productId, DateTime deadline)
        {
            var product = productRepository.Get(productId);
            if (product != null && product.Status > 0 && product.IsActive == true)
            {
                if (product.Status == 5)
                {
                    throw new UserException("Sản phẩm đã hoàn thành không thể thay đổi thời hạn");
                }

                var order = orderRepository.Get(product.OrderId);
                if (order != null && order.Status > 0 && order.IsActive == true)
                {
                    if (order.Status == 8)
                    {
                        throw new UserException("Hóa đơn đã bàn giao cho khách không thể thay đổi thời hạn");
                    }
                    var productStages = productStageRepository.GetAll(x => x.ProductId == productId && x.StageNum != null && x.Status >= 1 && x.Status < 4 && x.IsActive == true);
                    if (productStages != null && productStages.Any())
                    {
                        productStages = productStages.OrderBy(x => x.StageNum).ToList();

                        var smallIndexTask = productStages.First();

                        smallIndexTask.Deadline = deadline;

                        return productStageRepository.Update(smallIndexTask.Id, smallIndexTask);
                    }
                    else
                    {
                        var productTemplate = productTemplateRepository.Get(product.ProductTemplateId);
                        if (!string.IsNullOrWhiteSpace(product.SaveOrderComponents))
                        {
                            if (productTemplate != null)
                            {
                                var productTemplateStagees = templateStateRepository.GetAll(x => x.ProductTemplateId == productTemplate.Id && x.IsActive == true);
                                if (productTemplateStagees != null && productTemplateStagees.Any())
                                {
                                    productTemplateStagees = productTemplateStagees.ToList();

                                    product.ProductStages = new List<ProductStage>();

                                    var tasks = new List<Task>();
                                    foreach (var stage in productTemplateStagees)
                                    {
                                        tasks.Add(Task.Run(() =>
                                        {
                                            var productStage = new ProductStage()
                                            {
                                                Id = Ultils.GenGuidString(),
                                                Deadline = stage.StageNum == 1 ? deadline : null,
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

                                            var componentTypesInStage = componentStageRepository.GetAll(x => productTemplateStagees.Select(c => c.Id).Contains(x.TemplateStageId)); ;
                                            if (componentTypesInStage != null && componentTypesInStage.Any())
                                            {
                                                componentTypesInStage = componentTypesInStage.ToList();

                                                var componentsInStage = componentRepository.GetAll(x => productTemplate.Id == x.ProductTemplateId && x.IsActive == true);
                                                if (componentsInStage != null && componentsInStage.Any())
                                                {
                                                    componentsInStage = componentsInStage.ToList();

                                                    var productComponents = JsonConvert.DeserializeObject<List<ProductComponent>>(product.SaveOrderComponents);
                                                    if (productComponents != null && productComponents.Any())
                                                    {
                                                        var productComponent = productComponents.FirstOrDefault(x => componentsInStage.Select(c => c.Id).Contains(x.ComponentId));
                                                        if (productComponent != null)
                                                        {
                                                            productComponent.Id = Ultils.GenGuidString();
                                                            productComponent.LastestUpdatedTime = DateTime.Now;
                                                            productComponent.Name = componentsInStage.FirstOrDefault(x => x.Id == productComponent.ComponentId)?.Name;
                                                            productComponent.ProductStageId = productStage.Id;

                                                            productStage.ProductComponents.Add(productComponent);
                                                        }
                                                    }
                                                }
                                            }

                                            product.ProductStages.Add(productStage);
                                        }));
                                    }
                                    await Task.WhenAll(tasks);
                                }
                            }
                            else
                            {
                                throw new UserException("Không tìm thấy mẫu sản phẩm");
                            }
                            product.SaveOrderComponents = string.Empty;

                            return productRepository.Update(product.Id, product);
                        }
                        else
                        {
                            return false;
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
                throw new UserException("Không tìm thấy sản phẩm");
            }
        }

        public async Task<bool> AssignTaskToStaff(string productId, string staffId)
        {
            var product = productRepository.Get(productId);
            if (product != null && product.Status > 0 && product.Status < 5 && product.IsActive == true)
            {
                var order = orderRepository.Get(product.OrderId);
                if (order != null && order.Status > 0 && order.IsActive == true)
                {
                    if (product.Status == 2 && product.StaffMakerId != null)
                    {
                        throw new UserException("Sản phẩm đang được thực hiện. Vui lòng tạm dừng trước khi chuyển giao cho nhân viên khác");
                    }
                    var staff = staffRepository.Get(staffId);
                    if (staff != null && staff.IsActive == true)
                    {
                        await SwapTaskIndex(productId, staffId, null);

                        return true;
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy nhân viên");
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy hóa đơn");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy sản phẩm");
            }
        }

        public async Task<bool> UnAssignStaffTask(string productId, string staffId)
        {
            var staff = staffRepository.Get(staffId);
            if (staff != null && staff.IsActive == true)
            {
                var product = productRepository.Get(productId);
                if (product != null && product.Status > 0 && product.Status < 5 && product.IsActive == true)
                {
                    var order = orderRepository.Get(product.OrderId);
                    if (order != null && order.Status > 0 && order.IsActive == true)
                    {
                        var currentStaffTasks = productRepository.GetAll(x => x.Status > 0 && x.Status < 5 && x.StaffMakerId == staffId && x.Index != null && x.IsActive == true);
                        if (currentStaffTasks != null && currentStaffTasks.Any())
                        {
                            currentStaffTasks = currentStaffTasks.ToList();

                            if (currentStaffTasks.Select(x => x.Id).Contains(productId))
                            {
                                var task = currentStaffTasks.FirstOrDefault(x => x.Id == productId);

                                if (task != null)
                                {
                                    var currentTaskStages = productStageRepository.GetAll(x => x.ProductId == productId && x.Status > 0 && x.Status < 4 && x.IsActive == true);
                                    if (currentTaskStages != null && currentTaskStages.Any())
                                    {
                                        currentTaskStages = currentTaskStages.OrderBy(x => x.StageNum).ToList();

                                        var minStage = currentTaskStages.First();
                                        minStage.Status = 3;
                                        minStage.StaffId = null;

                                        task.StaffMakerId = null;
                                        task.StaffMaker = null;
                                        task.Index = null;
                                        task.Status = 3;
                                        if (productRepository.Update(task.Id, task))
                                        {
                                            if (productStageRepository.Update(minStage.Id, minStage))
                                            {
                                                await SwapTaskIndex(task.Id, null, null);
                                                return true;
                                            }
                                            else
                                            {
                                                throw new SystemsException("Lỗi trong quá trình hủy phân công công việc cho nhân viên", nameof(TaskService.UnAssignStaffTask));
                                            }
                                        }
                                        else
                                        {
                                            throw new SystemsException("Lỗi trong quá trình hủy phân công công việc cho nhân viên", nameof(TaskService.UnAssignStaffTask));
                                        }
                                    }
                                    else
                                    {
                                        task.StaffMakerId = null;
                                        task.StaffMaker = null;
                                        task.Index = null;
                                        task.Status = 3;
                                        if (productRepository.Update(task.Id, task))
                                        {
                                            await SwapTaskIndex(task.Id, null, null);
                                            return true;
                                        }
                                        else
                                        {
                                            throw new SystemsException("Lỗi trong quá trình hủy phân công công việc cho nhân viên", nameof(TaskService.UnAssignStaffTask));
                                        }
                                    }
                                }
                            }
                        }
                        throw new UserException("Không tìm thấy nhiệm vụ của nhân viên");
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy hóa đơn");
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy sản phẩm");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy nhân viên");
            }
        }

        public async Task SwapTaskIndex(string productId, string? staffId, int? index)
        {
            var product = productRepository.Get(productId);
            if (product != null && product.IsActive == true && product.Status != 0)
            {
                var order = orderRepository.Get(product.OrderId);
                if (order != null && order.IsActive == true && order.Status != 0)
                {
                    var listTasks = new List<Product>();
                    if (string.IsNullOrEmpty(staffId))
                    {
                        var unAssignTasks = productRepository.GetAll(x => x.Status > 0 && x.Status < 5 && x.StaffMakerId == null && x.Index != null && x.IsActive == true);
                        if (unAssignTasks != null && unAssignTasks.Any())
                        {
                            listTasks = unAssignTasks.OrderBy(x => x.Index).ToList();
                        }
                        else
                        {
                            listTasks = null;
                        }
                    }
                    else
                    {
                        var staff = staffRepository.Get(staffId);
                        if (staff == null || staff.IsActive == false)
                        {
                            throw new UserException("Không tìm thấy nhân viên");
                        }
                        else
                        {
                            var staffCurrentTask = productRepository.GetAll(x => x.Status > 0 && x.Status < 5 && x.StaffMakerId == staff.Id && x.Index != null && x.IsActive == true);
                            if (staffCurrentTask != null && staffCurrentTask.Any())
                            {
                                listTasks = staffCurrentTask.OrderBy(x => x.Index).ToList();
                            }
                            else
                            {
                                listTasks = null;
                            }
                        }
                    }

                    if (listTasks != null && listTasks.Any())
                    {
                        listTasks = listTasks.OrderBy(x => x.Index).ToList();

                        var minIndex = listTasks.OrderBy(x => x.Index).First().Index;
                        var maxIndex = listTasks.OrderByDescending(x => x.Index).First().Index;

                        if (index == null)
                        {
                            index = maxIndex + 1;
                        }

                        if (index < minIndex.Value - 1 || index > maxIndex.Value + 1)
                        {
                            throw new UserException("Thứ tự nhiệm vụ không hợp lệ");
                        }
                        else
                        {
                            if (listTasks.Select(x => x.Id).Contains(productId))
                            {
                                var oldTask = listTasks.First(x => x.Id == productId);

                                if (oldTask.Index.Value != index.Value)
                                {
                                    if (index <= minIndex)
                                    {
                                        var oldTaskMinIndex = listTasks.FirstOrDefault(x => x.Index == minIndex);
                                        if (oldTaskMinIndex != null)
                                        {
                                            if (oldTaskMinIndex.Status == 2)
                                            {
                                                listTasks = listTasks.OrderBy(x => x.Index).ToList();

                                                for (int i = 0; i < listTasks.Count; i++)
                                                {
                                                    if (i > 0)
                                                    {
                                                        if (listTasks[i].Id == productId)
                                                        {
                                                            listTasks[i].Index = minIndex + 1;
                                                        }
                                                        else
                                                        {
                                                            listTasks[i].Index = minIndex + 1 + i;
                                                        }
                                                        listTasks[i].StaffMakerId = staffId;
                                                        listTasks[i].LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                                    }
                                                }

                                                await productRepository.UpdateRangeProduct(listTasks);
                                            }
                                            else
                                            {
                                                oldTask.Index = minIndex;

                                                listTasks = listTasks.OrderBy(x => x.Index).ToList();
                                                for (int i = 0; i < listTasks.Count; i++)
                                                {
                                                    listTasks[i].Index = minIndex + 1 + i;
                                                    listTasks[i].StaffMakerId = staffId;
                                                    listTasks[i].LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                                }

                                                await productRepository.UpdateRangeProduct(listTasks);
                                            }
                                        }
                                    }
                                    else if (index == maxIndex)
                                    {
                                        var oldTaskIndex = listTasks.IndexOf(oldTask);
                                        if (oldTaskIndex > -1)
                                        {
                                            for (int i = oldTaskIndex + 1; i < listTasks.Count; i++)
                                            {
                                                listTasks[i].Index = listTasks[i].Index - 1;
                                                listTasks[i].StaffMakerId = staffId;
                                            }
                                            oldTask.Index = index;
                                            oldTask.StaffMakerId = staffId;

                                            listTasks = listTasks.OrderBy(x => x.Index).ToList();

                                            await productRepository.UpdateRangeProduct(listTasks);
                                        }
                                    }
                                    else if (index > maxIndex)
                                    {
                                        oldTask.Index = maxIndex + 1;
                                        oldTask.StaffMakerId = staffId;

                                        var oldTaskIndex = listTasks.IndexOf(oldTask);

                                        listTasks = listTasks.OrderBy(x => x.Index).ToList();
                                        if (oldTaskIndex > -1)
                                        {
                                            for (int i = oldTaskIndex; i < listTasks.Count; i++)
                                            {
                                                listTasks[i].Index = minIndex + i;
                                                listTasks[i].StaffMakerId = staffId;
                                            }

                                            await productRepository.UpdateRangeProduct(listTasks);
                                        }
                                    }
                                    else
                                    {
                                        var oldIndexTask = listTasks.FirstOrDefault(x => x.Index == index);
                                        if (oldIndexTask != null)
                                        {
                                            var oldIndexTaskIndex = listTasks.IndexOf(oldIndexTask);
                                            var oldTaskIndex = listTasks.IndexOf(oldTask);
                                            if (oldIndexTaskIndex > -1 && oldTaskIndex > -1)
                                            {
                                                if (oldTaskIndex < oldIndexTaskIndex)
                                                {
                                                    for (int i = oldTaskIndex + 1; i <= oldIndexTaskIndex; i++)
                                                    {
                                                        listTasks[i].Index = listTasks[i].Index - 1;
                                                        listTasks[i].StaffMakerId = staffId;
                                                    }
                                                }
                                                else if (oldTaskIndex > oldIndexTaskIndex)
                                                {
                                                    for (int i = oldIndexTaskIndex; i <= oldTaskIndex - 1; i++)
                                                    {
                                                        listTasks[i].Index = listTasks[i].Index + 1;
                                                        listTasks[i].StaffMakerId = staffId;
                                                    }
                                                }
                                            }
                                        }

                                        oldTask.Index = index;
                                        oldTask.StaffMakerId = staffId;

                                        listTasks = listTasks.OrderBy(x => x.Index).ToList();

                                        await productRepository.UpdateRangeProduct(listTasks);
                                    }
                                }
                            }
                            else
                            {
                                if (index <= minIndex)
                                {
                                    var oldTaskMinIndex = listTasks.FirstOrDefault(x => x.Index == minIndex);
                                    if (oldTaskMinIndex.Status == 2)
                                    {
                                        product.Index = minIndex + 1;

                                        listTasks.Insert(1, product);
                                        listTasks = listTasks.OrderBy(x => x.Index).ToList();
                                        for (int i = 2; i < listTasks.Count; i++)
                                        {
                                            listTasks[i].Index = minIndex + i - 1;
                                            listTasks[i].StaffMakerId = staffId;
                                        }

                                        await productRepository.UpdateRangeProduct(listTasks);
                                    }
                                    else
                                    {
                                        product.Index = minIndex - 1;

                                        listTasks.Insert(0, product);
                                        listTasks = listTasks.OrderBy(x => x.Index).ToList();
                                        for (int i = 0; i < listTasks.Count; i++)
                                        {
                                            listTasks[i].Index = minIndex + i;
                                            listTasks[i].StaffMakerId = staffId;
                                        }

                                        await productRepository.UpdateRangeProduct(listTasks);
                                    }
                                }
                                else if (index == maxIndex)
                                {
                                    var oldMaxIndex = listTasks.First(x => x.Index == maxIndex);

                                    oldMaxIndex.Index = maxIndex + 1;
                                    product.Index = maxIndex;

                                    product.StaffMakerId = staffId;
                                    oldMaxIndex.StaffMakerId = staffId;

                                    productRepository.Update(product.Id, product);
                                    productRepository.Update(oldMaxIndex.Id, oldMaxIndex);
                                }
                                else if (index > maxIndex)
                                {
                                    product.Index = maxIndex + 1;
                                    product.StaffMakerId = staffId;

                                    productRepository.Update(productId, product);
                                }
                                else
                                {
                                    var oldIndexTask = listTasks.First(x => x.Index == index);
                                    var oldIndexTaskIndex = listTasks.IndexOf(oldIndexTask);
                                    if (oldIndexTaskIndex > -1)
                                    {
                                        for (int i = oldIndexTaskIndex; i < listTasks.Count; i++)
                                        {
                                            listTasks[i].Index = minIndex + 1 + i;
                                            listTasks[i].StaffMakerId = staffId;
                                        }
                                    }
                                    product.Index = index;
                                    product.StaffMakerId = staffId;

                                    listTasks = listTasks.OrderBy(x => x.Index).ToList();

                                    await productRepository.UpdateRangeProduct(listTasks);
                                }
                            }
                        }
                    }
                    else
                    {
                        product.Index = 1;
                        product.StaffMakerId = staffId;

                        productRepository.Update(productId, product);
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy hóa đơn");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy sản phẩm");
            }
        }

        public async void ResetIndex(string? staffId)
        {
            var listTasks = new List<Product>();
            if (string.IsNullOrEmpty(staffId))
            {
                var unAssignTasks = productRepository.GetAll(x => x.Status > 0 && x.Status < 5 && x.StaffMakerId == null && x.Index != null && x.IsActive == true);
                if (unAssignTasks != null && unAssignTasks.Any())
                {
                    listTasks = unAssignTasks.OrderBy(x => x.Index).ToList();
                }
                else
                {
                    listTasks = null;
                }
            }
            else
            {
                var staff = staffRepository.Get(staffId);
                if (staff == null || staff.IsActive == false)
                {
                    throw new UserException("Không tìm thấy nhân viên");
                }
                else
                {
                    var staffCurrentTask = productRepository.GetAll(x => x.Status > 0 && x.Status < 5 && x.StaffMakerId == staff.Id && x.Index != null && x.IsActive == true);
                    if (staffCurrentTask != null && staffCurrentTask.Any())
                    {
                        listTasks = staffCurrentTask.OrderBy(x => x.Index).ToList();
                    }
                    else
                    {
                        listTasks = null;
                    }
                }
            }

            if (listTasks != null && listTasks.Any())
            {
                listTasks = listTasks.OrderBy(x => x.CreatedTime).ToList();
                for (int i = 0; i < listTasks.Count; i++)
                {
                    listTasks[i].Index = i + 1;
                    listTasks[i].StaffMakerId = staffId;
                }

                await productRepository.UpdateRangeProduct(listTasks);
            }
        }

        public async void ResetBlankIndex(string? staffId)
        {
            var listTasks = new List<Product>();
            if (string.IsNullOrEmpty(staffId))
            {
                var unAssignTasks = productRepository.GetAll(x => x.Status > 0 && x.Status < 5 && x.StaffMakerId == null && x.Index != null && x.IsActive == true);
                if (unAssignTasks != null && unAssignTasks.Any())
                {
                    listTasks = unAssignTasks.OrderBy(x => x.Index).ToList();
                }
                else
                {
                    listTasks = null;
                }
            }
            else
            {
                var staff = staffRepository.Get(staffId);
                if (staff == null || staff.IsActive == false)
                {
                    throw new UserException("Không tìm thấy nhân viên");
                }
                else
                {
                    var staffCurrentTask = productRepository.GetAll(x => x.Status > 0 && x.Status < 5 && x.StaffMakerId == staff.Id && x.Index != null && x.IsActive == true);
                    if (staffCurrentTask != null && staffCurrentTask.Any())
                    {
                        listTasks = staffCurrentTask.OrderBy(x => x.Index).ToList();
                    }
                    else
                    {
                        listTasks = null;
                    }
                }
            }

            if (listTasks != null && listTasks.Any())
            {
                listTasks = listTasks.OrderBy(x => x.CreatedTime).ToList();
                for (int i = 0; i < listTasks.Count; i++)
                {
                    listTasks[i].Index = i + 1;
                    listTasks[i].StaffMakerId = staffId;
                }

                var lastIndex = listTasks.LastOrDefault();

                lastIndex.StaffMakerId = null;
                lastIndex.StaffMaker = null;

                await productRepository.UpdateRangeProduct(listTasks);
            }
        }

        public async Task<bool> StartTask(string productId, string productStageId, string staffId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(productStageId))
                {
                    throw new UserException("Dữ liệu không hợp lệ");
                }
                var productIdParam = new SqlParameter("@ProductId", SqlDbType.NVarChar)
                {
                    Value = productId
                };
                var productStageIdParam = new SqlParameter("@ProductStageId", SqlDbType.NVarChar)
                {
                    Value = productStageId
                };
                var staffIdParam = new SqlParameter("@StaffId", SqlDbType.NVarChar)
                {
                    Value = staffId
                };
                var result = await productRepository.GetStoreProcedureReturnInt(StoreProcName.Start_Task
                    , productIdParam
                    , productStageIdParam
                    , staffIdParam);
                if (result == 1)
                {
                    return true;
                }
                else
                {
                    throw new SystemsException("Lỗi trong quá trình bắt đầu công việc", nameof(TaskService.StartTask));
                }
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }

        public async Task<bool> FinishTask(string wwwroot, string productId, string productStageId, string staffId, List<IFormFile>? images)
        {
            var imageNews = new List<string>();
            try
            {
                var stage = productStageRepository.Get(productStageId);
                if (stage != null && stage.IsActive == true)
                {
                    var product = productRepository.Get(productId);
                    if (product != null && product.IsActive == true)
                    {
                        var imageUrls = new List<string>();
                        if (!string.IsNullOrEmpty(stage.EvidenceImage))
                        {
                            imageUrls.AddRange(JsonConvert.DeserializeObject<List<string>>(stage.EvidenceImage));
                        }

                        if (images != null && images.Any())
                        {
                            var tasks2 = new List<Task>();
                            foreach (var image in images)
                            {
                                tasks2.Add(Task.Run(async () =>
                                {
                                    var imageObjectName = await Ultils.UploadImage(wwwroot, $"Product/{productId}/StageEvidences", image, null);
                                    imageNews.Add(imageObjectName);
                                }));
                            }
                            await Task.WhenAll(tasks2);
                        }
                        else
                        {
                            throw new UserException("Cần phải có ít nhất 1 hình ảnh chứng minh công đoạn");
                        }

                        if (imageNews != null && imageNews.Any())
                        {
                            imageUrls.AddRange(imageNews);
                        }

                        if (imageUrls != null && imageUrls.Any())
                        {
                            stage.EvidenceImage = JsonConvert.SerializeObject(imageUrls);
                        }
                        else
                        {
                            throw new UserException("Cần phải có ít nhất 1 hình ảnh chứng minh công đoạn");
                        }

                        if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(productStageId))
                        {
                            throw new UserException("Dữ liệu không hợp lệ");
                        }
                        var productIdParam = new SqlParameter("@ProductId", SqlDbType.NVarChar)
                        {
                            Value = productId
                        };
                        var productStageIdParam = new SqlParameter("@ProductStageId", SqlDbType.NVarChar)
                        {
                            Value = productStageId
                        };
                        var staffIdParam = new SqlParameter("@StaffId", SqlDbType.NVarChar)
                        {
                            Value = staffId
                        };
                        var imageParam = new SqlParameter("@EvidenceImage", SqlDbType.NVarChar)
                        {
                            Value = stage.EvidenceImage
                        };
                        var result = await productRepository.GetStoreProcedureReturnInt(StoreProcName.Finish_Task
                            , productIdParam
                            , productStageIdParam
                            , staffIdParam
                            , imageParam);
                        if (result == 1)
                        {
                            return true;
                        }
                        else if (result == 2)
                        {
                            var title = new SqlParameter("@Title", SqlDbType.NVarChar)
                            {
                                Value = $"Sản phẩm của đơn hàng {product.OrderId} đã hoàn thành."
                            };

                            var content = new SqlParameter("@Title", SqlDbType.NVarChar)
                            {
                                Value = $"Sản phẩm {product.Id} : {product.Name} của đơn hàng đã hoàn thành."
                            };

                            var returnValue = await notificationRepository.GetStoreProcedureReturnInt(StoreProcName.Create_Manager_Notification, title, content);

                            if (returnValue == 1)
                            {
                                await signalRService.SendNotificationToManagers(content.Value.ToString());

                                return true;
                            }
                            else
                            {
                                throw new SystemsException("Lỗi trong quá trình tạo thông báo", nameof(TaskService.FinishTask));
                            }
                        }
                        else if (result == 3)
                        {
                            var title = new SqlParameter("@Title", SqlDbType.NVarChar)
                            {
                                Value = $"Đơn hàng {product.OrderId} đã hoàn thành."
                            };

                            var content = new SqlParameter("@Title", SqlDbType.NVarChar)
                            {
                                Value = $"Các sản phẩm của đơn hàng {product.OrderId} đã hoàn thành. Quản lý vui lòng kiểm tra trước khi bàn giao cho khách kiểm thử."
                            };

                            var returnValue = await notificationRepository.GetStoreProcedureReturnInt(StoreProcName.Create_Manager_Notification, title, content);

                            if (returnValue == 1)
                            {
                                await signalRService.SendNotificationToManagers(content.Value.ToString());

                                return true;
                            }
                            else
                            {
                                throw new SystemsException("Lỗi trong quá trình tạo thông báo", nameof(TaskService.FinishTask));
                            }
                        }
                        else if (result == 4)
                        {
                            var title = new SqlParameter("@Title", SqlDbType.NVarChar)
                            {
                                Value = $"Đơn hàng {product.OrderId} cần sửa lại đã hoàn thành."
                            };

                            var content = new SqlParameter("@Title", SqlDbType.NVarChar)
                            {
                                Value = $"Các sản phẩm của đơn hàng {product.OrderId} cần sửa lại đã hoàn thành. Quản lý vui lòng kiểm tra trước khi bàn giao cho khách kiểm thử."
                            };

                            var returnValue = await notificationRepository.GetStoreProcedureReturnInt(StoreProcName.Create_Manager_Notification, title, content);

                            if (returnValue == 1)
                            {
                                await signalRService.SendNotificationToManagers(content.Value.ToString());

                                return true;
                            }
                            else
                            {
                                throw new SystemsException("Lỗi trong quá trình tạo thông báo", nameof(TaskService.FinishTask));
                            }
                        }
                        else
                        {
                            throw new SystemsException("Lỗi trong quá trình kết thúc công việc", nameof(TaskService.StartTask));
                        }
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy sản phẩm");
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy công đoạn");
                }
            }
            catch (UserException ex)
            {
                if (imageNews != null && imageNews.Any())
                {
                    foreach (var image in imageNews)
                    {
                        Ultils.DeleteObject(JsonConvert.DeserializeObject<ImageFileDTO>(image).ObjectName);
                    }
                }
                throw new UserException(ex.Message);
            }
            catch (SystemsException ex)
            {
                if (imageNews != null && imageNews.Any())
                {
                    foreach (var image in imageNews)
                    {
                        Ultils.DeleteObject(JsonConvert.DeserializeObject<ImageFileDTO>(image).ObjectName);
                    }
                }
                throw new SystemsException(ex.Message, nameof(TaskService.FinishTask));
            }
            catch (Exception ex)
            {
                if (imageNews != null && imageNews.Any())
                {
                    foreach (var image in imageNews)
                    {
                        Ultils.DeleteObject(JsonConvert.DeserializeObject<ImageFileDTO>(image).ObjectName);
                    }
                }
                throw new UserException(ex.Message);
            }
        }

        public bool PendingTask(string productId, string productStageId, string staffId)
        {
            var product = productRepository.Get(productId);

            if (product != null && product.IsActive == true && product.Status != 0 && product.StaffMakerId == staffId)
            {
                switch (product.Status)
                {
                    case 0:
                        throw new UserException("Sản phẩm bị hủy");
                    case 1:
                        throw new UserException("Sản phẩm đang chờ");
                    case 5:
                        throw new UserException("Sản phẩm đã hoàn thành");
                }
                var order = orderRepository.Get(product.OrderId);

                if (order != null && order.Status != 0 && order.IsActive == true)
                {
                    switch (order.Status)
                    {
                        case 0:
                            throw new UserException("Hóa đơn bị hủy");
                        case 1:
                            throw new UserException("Hóa đơn chưa được duyệt");
                        case 2:
                            throw new UserException("Hóa đơn chưa được bắt đầu");
                        case 3:
                            throw new UserException("Hóa đơn chưa bắt đầu");
                        case 5:
                            throw new UserException("Hóa đơn bị tạm dừng");
                        case 6:
                            throw new UserException("Hóa đơn đang chờ khách kiểm duyệt");
                        case 8:
                            throw new UserException("Hóa đơn đã hoàn thành");
                    }
                    var orderProducts = productRepository.GetAll(x => x.OrderId == product.OrderId && x.Status != 0 && x.IsActive == true);
                    if (orderProducts != null && orderProducts.Any())
                    {
                        orderProducts = orderProducts.ToList();

                        var productStages = productStageRepository.GetAll(x => x.IsActive == true && x.ProductId == productId && x.Status != 0);
                        if (productStages != null && productStages.Any())
                        {
                            var productStage = productStages.FirstOrDefault(x => x.Id == productStageId);

                            switch (productStage.Status)
                            {
                                case 0:
                                    throw new UserException("Công đoạn chưa được bắt đầu");
                                case 1:
                                    throw new UserException("Công đoạn đang chờ");
                                case 3:
                                    throw new UserException("Công đoạn đang tạm dừng");
                                case 4:
                                    throw new UserException("Công đoạn đã hoàn thành");
                            }

                            if (productStage != null && productStage.IsActive == true && productStage.Status != 0 && productStage.ProductId == productId && productStage.StaffId == staffId)
                            {
                                product.Status = 3;
                                product.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

                                productRepository.Update(product.Id, product);

                                productStage.Status = 3;
                                productStage.StaffId = staffId;

                                return productStageRepository.Update(productStage.Id, productStage);
                            }
                            else
                            {
                                throw new UserException("Không tìm thấy nhiệm vụ");
                            }
                        }
                        else
                        {
                            throw new UserException($"Không tìm thấy danh sách nhiệm vụ của sản phẩm: {product.Name}");
                        }
                    }
                    else
                    {
                        throw new UserException("Không tìm thấy sản phẩm hóa đơn");
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy hóa đơn");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy sản phẩm");
            }
        }

        public async Task<bool> DefectsTask(string productId, string orderId)
        {
            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true)
            {
                switch (order.Status)
                {
                    case 0:
                        throw new UserException("Hóa đơn bị hủy");
                    case 1:
                        throw new UserException("Hóa đơn chưa được duyệt");
                    case 2:
                        throw new UserException("Hóa đơn chưa được bắt đầu");
                    case 3:
                        throw new UserException("Hóa đơn chưa bắt đầu");
                    case 8:
                        throw new UserException("Hóa đơn đã hoàn thành");
                }

                var product = productRepository.Get(productId);
                if (product != null && product.IsActive == true && product.Status != 0 && product.OrderId == orderId)
                {
                    if (product.Status != 5)
                    {
                        throw new UserException("Sản phẩm chưa hoàn thành");
                    }
                    else
                    {
                        var title = new SqlParameter("@Title", SqlDbType.NVarChar);

                        var content = new SqlParameter("@Title", SqlDbType.NVarChar);

                        product.Status = 4;

                        var productStages = productStageRepository.GetAll(x => x.ProductId == productId && x.IsActive == true && x.Status > 0);
                        if (productStages != null && productStages.Any())
                        {
                            productStages = productStages.ToList();

                            foreach (var stage in productStages)
                            {
                                stage.Status = 1;

                                productStageRepository.Update(stage.Id, stage);
                            }
                        }
                        else
                        {
                            throw new UserException("Không tìm thấy công đoạn của sản phẩm");
                        }

                        if (order.Status == 6)
                        {
                            order.Status = 7;

                            title.Value = $"Hóa đơn {order.Id} vừa bị khách từ chối.";
                            content.Value = $"Sản phẩm {product.Id} : {product.Name} bị khách từ chối.";
                        }
                        else
                        {
                            title.Value = $"Hóa đơn {order.Id} vừa bị quản lý từ chối.";
                            content.Value = $"Sản phẩm {product.Id} : {product.Name} bị quản lý từ chối.";
                        }

                        var notify = new Notification
                        {
                            Title = title.Value.ToString(),
                            Content = content.Value.ToString(),
                            StaffId = product.StaffMakerId,
                            Id = Ultils.GenGuidString(),
                            SendTime = DateTime.UtcNow.AddHours(7),
                            IsRead = false,
                            ReadTime = null,
                            IsActive = true
                        };

                        if (notificationRepository.Create(notify))
                        {
                            await signalRService.SendNotificationToUser(notify.StaffId, notify.Title);
                        }
                        else
                        {
                            throw new SystemsException("Lỗi trong quá trình tạo thông báo", nameof(TaskService.DefectsTask));
                        }

                        if (order.Status == 6)
                        {
                            var returnValue = await notificationRepository.GetStoreProcedureReturnInt(StoreProcName.Create_Manager_Notification, title, content);

                            if (returnValue == 1)
                            {
                                await signalRService.SendNotificationToManagers(title.Value.ToString());
                            }
                            else
                            {
                                throw new SystemsException("Lỗi trong quá trình tạo thông báo", nameof(TaskService.FinishTask));
                            }
                        }

                        if (order.Status == 6)
                        {
                            if (orderRepository.Update(orderId, order))
                            {
                                return productRepository.Update(productId, product);
                            }
                            else
                            {
                                throw new SystemsException("Lỗi trong quá trình từ chối sản phẩm", nameof(TaskService.DefectsTask));
                            }
                        }
                        else
                        {
                            return productRepository.Update(productId, product);
                        }
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

        public async Task<IEnumerable<Category>> GetTaskByCategories()
        {
            var activeOrders = orderRepository.GetAll(x => x.Status >= 3 && x.Status <= 7 && x.IsActive == true);
            if (activeOrders != null && activeOrders.Any())
            {
                activeOrders = activeOrders.ToList();

                var orderProducts = productRepository.GetAll(x => activeOrders.Select(c => c.Id).Contains(x.OrderId) && x.Status >= 1 && x.Status <= 5 && x.IsActive == true);
                if (orderProducts != null && orderProducts.Any())
                {
                    orderProducts = orderProducts.ToList();

                    var templates = productTemplateRepository.GetAll(x => orderProducts.Select(c => c.ProductTemplateId).Contains(x.Id));
                    if (templates != null && templates.Any())
                    {
                        templates = templates.ToList();

                        var categories = categoryRepository.GetAll(x => templates.Select(c => c.CategoryId).Contains(x.Id) || x.IsActive == true);
                        if (categories != null && categories.Any())
                        {
                            categories = categories.ToList();

                            var templateStages = templateStateRepository.GetAll(x => templates.Select(c => c.Id).Contains(x.ProductTemplateId) && x.IsActive == true);
                            if (templateStages != null && templateStages.Any())
                            {
                                templateStages = templateStages.ToList();

                                var productStages = productStageRepository.GetAll(x => x.IsActive == true && orderProducts.Select(c => c.Id).Contains(x.ProductId) && x.Status >= 1 && x.Status < 4);
                                if (productStages != null && productStages.Any())
                                {
                                    productStages = productStages.ToList();

                                    var staffs = staffRepository.GetAll(x => orderProducts.Select(x => x.StaffMakerId).Contains(x.Id) && x.IsActive == true);

                                    if (staffs != null && staffs.Any())
                                    {
                                        staffs = staffs.ToList();
                                    }

                                    var fabricMaterialIds = string.Join(",", orderProducts.Select(x => x.FabricMaterialId).Distinct().ToList());

                                    var fabricMaterials = materialRepository.GetAll(x => fabricMaterialIds.Contains(x.Id));

                                    if (fabricMaterials != null && fabricMaterials.Any())
                                    {
                                        fabricMaterials = fabricMaterials.ToList();
                                    }

                                    var tasks = new List<Task>();
                                    foreach (var category in categories)
                                    {
                                        tasks.Add(Task.Run(async () =>
                                        {
                                            category.ProductTemplates = templates.Where(x => x.CategoryId == category.Id)?.ToList();

                                            if (category.ProductTemplates != null && category.ProductTemplates.Any())
                                            {
                                                var categoryTasks = new List<Task>();
                                                foreach (var template in category.ProductTemplates)
                                                {
                                                    categoryTasks.Add(Task.Run(async () =>
                                                    {
                                                        template.Products.Clear();
                                                        template.Products = orderProducts.Where(x => x.ProductTemplateId == template.Id)?.ToList();

                                                        if (template.Products != null && template.Products.Any())
                                                        {
                                                            var templateTasks = new List<Task>();
                                                            foreach (var product in template.Products)
                                                            {
                                                                templateTasks.Add(Task.Run(async () =>
                                                                {
                                                                    if (product.StaffMakerId != null && staffs != null && staffs.Any())
                                                                    {
                                                                        product.StaffMaker = staffs.SingleOrDefault(x => x.Id == product.StaffMakerId);

                                                                        if (product.StaffMaker?.Avatar != null)
                                                                        {
                                                                            product.StaffMaker.Avatar = Ultils.GetUrlImage(product.StaffMaker.Avatar);
                                                                        }
                                                                    }
                                                                    if (product.FabricMaterial == null)
                                                                    {
                                                                        product.FabricMaterial = fabricMaterials.FirstOrDefault(x => x.Id == product.FabricMaterialId);
                                                                    }

                                                                    if (product.FabricMaterial != null)
                                                                    {
                                                                        product.FabricMaterial.Image = Ultils.GetUrlImage(product.FabricMaterial.Image);
                                                                    }

                                                                    product.ProductStages.Clear();
                                                                    product.ProductStages.Add(productStages.Where(x => x.ProductId == product.Id)?.OrderBy(x => x.StageNum)?.FirstOrDefault());
                                                                }));
                                                            }
                                                            await Task.WhenAll(templateTasks);

                                                            template.Products = template.Products?.OrderBy(x => x.CreatedTime)?.ToList();
                                                        }

                                                        template.TemplateStages.Clear();
                                                        template.TemplateStages = templateStages.Where(x => x.ProductTemplateId == template.Id)?.OrderBy(x => x.StageNum)?.ToList();
                                                    }));
                                                }
                                                await Task.WhenAll(categoryTasks);

                                                category.ProductTemplates = category.ProductTemplates?.OrderBy(x => x.Name)?.ToList();
                                            }
                                        }));
                                    }
                                    await Task.WhenAll(tasks);
                                }
                            }

                            return categories;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> SetMaterialForTask(string taskId, string stageId, List<ProductStageMaterial> stageMaterials)
        {
            if (stageMaterials != null && stageMaterials.Any())
            {
                var paramsSql = new List<SqlParameter>();

                var dataTable = new DataTable();
                while (dataTable.Rows != null && dataTable.Rows.Count != stageMaterials.Count)
                {
                    dataTable = new DataTable();
                    if (paramsSql.Find(x => x.ParameterName == "@StageMaterials") != null)
                    {
                        paramsSql.Remove(paramsSql.Find(x => x.ParameterName == "@StageMaterials"));
                    }

                    dataTable.Columns.Add("MaterialId", typeof(string));
                    dataTable.Columns.Add("Value", typeof(decimal));

                    var insideTasks = new List<Task>();

                    foreach (var stageMaterial in stageMaterials)
                    {
                        dataTable.Rows.Add(!string.IsNullOrEmpty(stageMaterial.MaterialId) ? stageMaterial.MaterialId : DBNull.Value, stageMaterial.Quantity.HasValue ? stageMaterial.Quantity : DBNull.Value);
                    }
                    await Task.WhenAll(insideTasks);

                    paramsSql.Add(new SqlParameter("@StageMaterials", SqlDbType.Structured)
                    {
                        TypeName = "dbo.MaterialStageType",
                        Value = dataTable
                    });
                };

                paramsSql.Add(new SqlParameter("@StageId", SqlDbType.NVarChar)
                {
                    Value = stageId
                });

                paramsSql.Add(new SqlParameter("@TaskId", SqlDbType.NVarChar)
                {
                    Value = taskId
                });

                try
                {
                    var result = await productRepository.GetStoreProcedureReturnInt(StoreProcName.Set_Material_For_Task,
                        paramsSql.Find(x => x.ParameterName == "@TaskId"),
                        paramsSql.Find(x => x.ParameterName == "@StageId"),
                        paramsSql.Find(x => x.ParameterName == "@StageMaterials")
                        );

                    if (result == 1)
                    {
                        return true;
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật nguyên vật liệu cho công đoạn", nameof(TaskService.SetMaterialForTask));
                    }
                }
                catch (Exception ex)
                {
                    throw new UserException(ex.Message);
                }
            }
            else
            {
                throw new UserException("Không có vật liệu");
            }
        }
    }
}
