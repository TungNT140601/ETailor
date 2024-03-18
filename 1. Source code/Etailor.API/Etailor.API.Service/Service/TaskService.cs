using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly IProductBodySizeRepository productBodySizeRepository;
        private readonly IBodySizeRepository bodySizeRepository;
        private readonly IMaterialRepository materialRepository;
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        private readonly IMaterialTypeRepository materialTypeRepository;

        public TaskService(IProductRepository productRepository, IProductStageRepository productStageRepository
            , IProductComponentRepository productComponentRepository, IOrderRepository orderRepository
            , IProductTemplateRepository productTemplateRepository, IComponentRepository componentRepository
            , IProductBodySizeRepository productBodySizeRepository, IBodySizeRepository bodySizeRepository
            , IMaterialCategoryRepository materialCategoryRepository, IMaterialRepository materialRepository, IMaterialTypeRepository materialTypeRepository)
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

                if (!(order != null && order.Status > 2 && order.IsActive == true))
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
                tasks.Add(Task.Run(async () =>
                {
                    if (product.FabricMaterial == null)
                    {
                        product.FabricMaterial = fabricMaterial;
                    }
                    if (!string.IsNullOrEmpty(product.FabricMaterial.Image) && !product.FabricMaterial.Image.StartsWith("https://firebasestorage.googleapis.com"))
                    {
                        product.FabricMaterial.Image = await Ultils.GetUrlImage(product.FabricMaterial.Image);
                    }
                    else if (product.FabricMaterial.Image.StartsWith("https://firebasestorage.googleapis.com/"))
                    {

                    }
                    else
                    {
                        product.FabricMaterial.Image = string.Empty;
                    }
                }));
                tasks.Add(Task.Run(async () =>
                {
                    if (product.ProductTemplate == null)
                    {
                        product.ProductTemplate = template;
                    }
                    if (!string.IsNullOrEmpty(product.ProductTemplate.ThumbnailImage) && !product.ProductTemplate.ThumbnailImage.StartsWith("https://firebasestorage.googleapis.com/"))
                    {
                        product.ProductTemplate.ThumbnailImage = await Ultils.GetUrlImage(product.ProductTemplate.ThumbnailImage);
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

                        var productComponents = productComponentRepository.GetAll(x => productStages != null && productStages.Any() && productStages.Select(c => c.Id).Contains(x.ProductStageId));

                        if (productComponents != null && productComponents.Any())
                        {
                            productComponents = productComponents.ToList();

                            var productTemplateComponents = componentRepository.GetAll(x => productComponents != null && productComponents.Any() && productComponents.Select(c => c.ComponentId).Contains(x.Id));

                            if (productTemplateComponents != null && productTemplateComponents.Any())
                            {
                                productTemplateComponents = productTemplateComponents.ToList();

                                var productBodySizes = productBodySizeRepository.GetAll(x => product != null && x.ProductId == productId);

                                if (productBodySizes != null && productBodySizes.Any())
                                {
                                    productBodySizes = productBodySizes.ToList();

                                    var bodySizes = bodySizeRepository.GetAll(x => productBodySizes != null && productBodySizes.Any() && productBodySizes.Select(c => c.BodySizeId).Contains(x.Id));

                                    if (bodySizes != null && bodySizes.Any())
                                    {
                                        bodySizes = bodySizes.ToList();

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
                                                    insideTasks2.Add(Task.Run(() =>
                                                    {
                                                        if (stage.ProductComponents == null)
                                                        {
                                                            stage.ProductComponents = productComponents.Where(x => x.ProductStageId == stage.Id).ToList();
                                                        }
                                                    }));
                                                    foreach (var component in stage.ProductComponents)
                                                    {
                                                        insideTasks2.Add(Task.Run(async () =>
                                                        {
                                                            if (component.Component == null)
                                                            {
                                                                component.Component = productTemplateComponents.FirstOrDefault(x => x.Id == component.ComponentId);
                                                            }
                                                            if (!string.IsNullOrEmpty(component.Component.Image) && !component.Component.Image.StartsWith("https://firebasestorage.googleapis.com/"))
                                                            {
                                                                component.Component.Image = await Ultils.GetUrlImage(component.Component.Image);
                                                            }
                                                            else if (component.Component.Image.StartsWith("https://firebasestorage.googleapis.com/"))
                                                            {

                                                            }
                                                            else
                                                            {
                                                                component.Component.Image = string.Empty;
                                                            }

                                                        }));
                                                    }
                                                    await Task.WhenAll(insideTasks2);
                                                }));
                                            }
                                            await Task.WhenAll(insideTasks1);
                                        }));
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

                                        return product;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public async Task<IEnumerable<ProductStage>> GetProductStagesOfEachTask(string taskId)
        {
            IEnumerable<ProductStage> ListOfProductStage = productStageRepository.GetAll(x => (taskId != null && x.ProductId == taskId) && x.IsActive == true).OrderBy(x => x.StageNum);
            return ListOfProductStage;
        }

        //For manager to get all
        public async Task<IEnumerable<Product>> GetTasks()
        {
            IEnumerable<Product> ListOfTask = productRepository.GetAll(x => x.IsActive == true);
            //foreach (Blog blog in ListOfBlog)
            //{
            //    var setThumbnail = Task.Run(async () =>
            //    {
            //        if (string.IsNullOrEmpty(blog.Thumbnail))
            //        {
            //            blog.Thumbnail = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
            //        }
            //        else
            //        {
            //            blog.Thumbnail = await Ultils.GetUrlImage(blog.Thumbnail);
            //        }
            //    });
            //    await Task.WhenAll(setThumbnail);
            //};
            return ListOfTask;
        }
        public async Task<IEnumerable<Product>> GetTasksByStaffId(string? search)
        {
            IEnumerable<Product> ListOfTask = productRepository.GetAll(x => (search == null || (search != null && x.StaffMakerId == search)) && x.IsActive == true);
            //foreach (Blog blog in ListOfBlog)
            //{
            //    var setThumbnail = Task.Run(async () =>
            //    {
            //        if (string.IsNullOrEmpty(blog.Thumbnail))
            //        {
            //            blog.Thumbnail = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
            //        }
            //        else
            //        {
            //            blog.Thumbnail = await Ultils.GetUrlImage(blog.Thumbnail);
            //        }
            //    });
            //    await Task.WhenAll(setThumbnail);
            //};
            return ListOfTask;
        }

        public async Task<bool> StartTask(string productId, string productStageId, string staffId)
        {
            var product = productRepository.Get(productId);
            var order = product != null ? orderRepository.Get(product?.OrderId) : null;
            var orderProducts = productRepository.GetAll(x => product != null && product.OrderId != null && x.OrderId == product.OrderId && x.Status != 0 && x.IsActive == true);
            var productStages = productStageRepository.GetAll(x => x.IsActive == true && x.ProductId == productId && x.Status != 0);

            var tasks = new List<Task>();
            if (product != null && product.IsActive == true && product.Status != 0)
            {
                if (order != null && order.Status != 0 && order.IsActive == true)
                {
                    if (orderProducts != null && orderProducts.Any())
                    {
                        orderProducts = orderProducts.ToList();
                        if (productStages != null && productStages.Any())
                        {
                            productStages = productStages.ToList();

                            var productStage = new ProductStage();

                            tasks.Add(Task.Run(() =>
                            {
                                productStage = productStages.First(x => x.Id == productStageId);
                            }));

                            await Task.WhenAll(tasks);

                            if (productStage != null && productStage.IsActive == true && productStage.Status != 0 && productStage.ProductId == productId && productStage.StaffId == staffId)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    product.Status = 2;
                                    product.LastestUpdatedTime = DateTime.UtcNow;
                                    productRepository.Update(product.Id, product);
                                }));

                                await Task.WhenAll(tasks);

                                if (!orderProducts.Any(x => x.Id != product.Id && x.Status > 1 && x.IsActive == true))
                                {
                                    tasks.Add(Task.Run(() =>
                                    {
                                        order.Status = 4;
                                        order.LastestUpdatedTime = DateTime.UtcNow;
                                        orderRepository.Update(order.Id, order);
                                    }));

                                    await Task.WhenAll(tasks);
                                }

                                tasks.Add(Task.Run(() =>
                                {
                                    productStage.Status = 2;
                                    productStage.StaffId = staffId;
                                    productStage.FinishTime = DateTime.UtcNow;
                                }));

                                await Task.WhenAll(tasks);

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
        public async Task<bool> FinishTask(string wwwroot, string productId, string productStageId, string staffId, List<IFormFile>? images)
        {
            var product = productRepository.Get(productId);
            var order = orderRepository.Get(product != null ? product.OrderId : null);
            var orderProducts = productRepository.GetAll(x => product != null && product.OrderId != null && x.OrderId == product.OrderId && x.Status != 0 && x.IsActive == true);
            var productStages = productStageRepository.GetAll(x => x.IsActive == true && x.ProductId == productId && x.Status != 0);

            var tasks = new List<Task>();
            if (product != null && product.IsActive == true && product.Status != 0)
            {
                if (order != null && order.Status != 0 && order.IsActive == true)
                {
                    if (orderProducts != null && orderProducts.Any())
                    {
                        orderProducts = orderProducts.ToList();
                        if (productStages != null && productStages.Any())
                        {
                            productStages = productStages.ToList();
                            int? maxStageNum = 0;
                            var productStage = new ProductStage();
                            tasks.Add(Task.Run(() =>
                            {
                                maxStageNum = productStages.OrderByDescending(x => x.StageNum).First().StageNum;
                            }));
                            tasks.Add(Task.Run(() =>
                            {
                                productStage = productStages.FirstOrDefault(x => x.Id == productStageId);
                            }));

                            await Task.WhenAll(tasks);

                            if (productStage != null && productStage.IsActive == true && productStage.Status != 0 && productStage.ProductId == productId && productStage.StaffId == staffId)
                            {
                                if (maxStageNum != null && productStage.StageNum == maxStageNum)
                                {
                                    tasks.Add(Task.Run(async () =>
                                    {
                                        product.Status = 4;
                                        product.LastestUpdatedTime = DateTime.UtcNow;

                                        productRepository.Update(product.Id, product);
                                    }));

                                    await Task.WhenAll(tasks);

                                    if (!orderProducts.Any(x => x.Id != product.Id && x.Status < 4 && x.IsActive == true))
                                    {
                                        tasks.Add(Task.Run(() =>
                                        {
                                            order.Status = 5;
                                            order.LastestUpdatedTime = DateTime.UtcNow;
                                            orderRepository.Update(order.Id, order);
                                        }));
                                    }
                                }

                                tasks.Add(Task.Run(async () =>
                                {
                                    if (images != null && images.Any())
                                    {
                                        var imageUrls = new List<string>();

                                        var tasks2 = new List<Task>();
                                        foreach (var image in images)
                                        {
                                            tasks2.Add(Task.Run(async () =>
                                            {
                                                var imageObjectName = await Ultils.UploadImage(wwwroot, "ProductStageEvidences", image, null);
                                                imageUrls.Add(imageObjectName);
                                            }));
                                        }
                                        await Task.WhenAll(tasks2);

                                        productStage.EvidenceImage = JsonConvert.SerializeObject(imageUrls);
                                    }
                                    else
                                    {
                                        throw new UserException("Cần phải có ít nhất 1 hình ảnh chứng minh công đoạn");
                                    }
                                }));

                                tasks.Add(Task.Run(() =>
                                {
                                    productStage.Status = 4;
                                    productStage.StaffId = staffId;
                                    productStage.FinishTime = DateTime.UtcNow;
                                }));

                                await Task.WhenAll(tasks);

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
        public bool PendingTask(string productId, string productStageId, string staffId)
        {
            var product = productRepository.Get(productId);
            var order = orderRepository.Get(product != null ? product.OrderId : null);
            var orderProducts = productRepository.GetAll(x => product != null && product.OrderId != null && x.OrderId == product.OrderId && x.Status != 0 && x.IsActive == true);
            var productStages = productStageRepository.GetAll(x => x.IsActive == true && x.ProductId == productId && x.Status != 0);

            if (product != null && product.IsActive == true && product.Status != 0)
            {
                if (order != null && order.Status != 0 && order.IsActive == true)
                {
                    if (orderProducts != null && orderProducts.Any())
                    {
                        orderProducts = orderProducts.ToList();
                        if (productStages != null && productStages.Any())
                        {
                            var productStage = productStages.FirstOrDefault(x => x.Id == productStageId);

                            if (productStage != null && productStage.IsActive == true && productStage.Status != 0 && productStage.ProductId == productId && productStage.StaffId == staffId)
                            {
                                product.Status = 3;
                                product.LastestUpdatedTime = DateTime.UtcNow;

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
    }
}
