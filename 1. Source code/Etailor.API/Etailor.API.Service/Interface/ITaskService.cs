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
        Task<bool> CreateTask(MaterialType materialType);
        Task<bool> UpdateTask(MaterialType materialType);
        bool DeleteTask(string id);
        MaterialType GetTask(string id);
        IEnumerable<MaterialType> GetTasks(string? search);
    }
}
