using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using System;
using System.Collections.Generic;
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

        public TaskService(IProductRepository productRepository, IProductStageRepository productStageRepository, IProductComponentRepository productComponentRepository, IOrderRepository orderRepository)
        {
            this.productRepository = productRepository;
            this.productStageRepository = productStageRepository;
            this.productComponentRepository = productComponentRepository;
            this.orderRepository = orderRepository;
        }

        public async Task<Product> GetTask(string id)
        {
            var product = productRepository.Get(id);

            var setThumbnail = Task.Run(async () =>
            {

            });
            await Task.WhenAll(setThumbnail);

            return product == null ? null : product.IsActive == true ? product : null;
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
        public async Task<bool> FinishTask(string productId, string productStageId, string staffId)
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
                                    tasks.Add(Task.Run(() =>
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
    }
}
