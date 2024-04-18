using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IComponentService
    {
        Task<string> AddComponent(Component component, IFormFile? image, string wwwroot);
        Task<string> UpdateComponent(Component component, IFormFile? newImage, string wwwroot);
        bool DeleteComponent(string id);
        Task<IEnumerable<Component>> GetAllByComponentType(string componentTypeId, string templateId);
        Task<bool> CheckDefaultComponent(string templateId);
        Task<Component> GetComponent(string id);
        Task<IEnumerable<Component>> GetComponents();
        Task<bool> ImportFileAddComponents(string templateId, IFormFile file, string wwwroot);
    }
}
