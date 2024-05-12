using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
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
        Task<bool> StartTask(string productId, string productStageId, string staffId);
        Task<bool> FinishTask(string wwwroot, string productId, string productStageId, string staffId, List<IFormFile>? images);
        bool PendingTask(string productId, string productStageId, string staffId);
        Task<bool> DefectsTask(string productId, string orderId);
        Task AutoCreateEmptyTaskProduct();
        void AutoAssignTaskForStaff();
        Task SwapTaskIndex(string productId, string? staffId, int? index);
        void ResetIndex(string? staffId);
        void ResetBlankIndex(string? staffId);
        Task<bool> SetDeadlineForTask(string productId, DateTime deadline);
        Task<bool> AssignTaskToStaff(string productId, string staffId);
        Task<bool> UnAssignStaffTask(string productId, string staffId);
        Task<IEnumerable<Category>> GetTaskByCategories();
        Task<bool> SetMaterialForTask(string taskId, string stageId, List<ProductStageMaterial> stageMaterials);
    }
}
