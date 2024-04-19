using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IProductTemplateService
    {
        Task<IEnumerable<ProductTemplate>> GetByCategory(string id);
        Task<IEnumerable<ProductTemplate>> GetByCategorys(List<string> ids);
        Task<ProductTemplate> GetByUrlPath(string urlPath);
        Task<ProductTemplate> GetById(string id);
        Task<string> AddTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? images, List<IFormFile>? collectionImages);
        Task<string> UpdateTemplate(string wwwroot, ProductTemplate productTemplate, IFormFile? thumbnailImage, List<IFormFile>? images, List<string>? existOldImages, List<IFormFile>? collectionImages, List<string>? existOldCollectionImages);
        bool CreateSaveActiveTemplate(string id);
        bool DeleteTemplate(string id);
        Task<IEnumerable<ComponentType>> GetTemplateComponent(string templateId);
        Task<IEnumerable<ProductTemplate>> GetTemplates(string? search);
        string ExportFile(string templateId);
    }
}
