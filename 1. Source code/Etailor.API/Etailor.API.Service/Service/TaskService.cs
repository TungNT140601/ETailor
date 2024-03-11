using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
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

        public TaskService(IProductRepository productRepository, IProductStageRepository productStageRepository)
        {
            this.productRepository = productRepository;
            this.productStageRepository = productStageRepository;
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

        public async Task<IEnumerable<ProductStage>> GetProductStagesOfEachTask (string taskId) 
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
    }
}
