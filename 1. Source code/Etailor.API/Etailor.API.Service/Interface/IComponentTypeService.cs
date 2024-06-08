using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IComponentTypeService
    {
        Task<bool> AddComponentType(ComponentType componentType);
        Task<bool> UpdateComponentType(ComponentType componentType); 
        Task<List<ComponentType>> AddComponentTypes(string categoryId, List<ComponentType> componentTypes);
        bool DeleteComponentType(string id);
        ComponentType GetComponentType(string id);
        IEnumerable<ComponentType> GetComponentTypes(string? search);
        IEnumerable<ComponentType> GetComponentTypesByCategory(string? id);
    }
}
