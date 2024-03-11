using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ITaskService
    {
        //Task<bool> CreateTask(ProductStage productStage);
        //Task<bool> UpdateTask(ProductStage productStage);
        //bool DeleteTask(string id);
        //ProductStage GetTask(string id);
        Task<IEnumerable<ProductStage>> GetProductStagesOfEachTask(string taskId);
        Task<Product> GetTask(string id);
        Task<IEnumerable<Product>> GetTasks();
        Task<IEnumerable<Product>> GetTasksByStaffId(string? search);
    }
}
