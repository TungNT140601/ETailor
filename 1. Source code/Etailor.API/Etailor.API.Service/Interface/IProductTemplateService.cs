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
        Task<ProductTemplate> GetByUrlPath(string urlPath);
        Task<string> AddTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? images, List<IFormFile>? collectionImages);
        Task<string> UpdateDraftTemplate(ProductTemplate productTemplate, string wwwroot, IFormFile? thumbnailImage, List<IFormFile>? newImages, List<IFormFile>? newCollectionImages);
        bool CreateSaveActiveTemplate(string id);
        Task<string> UpdateTemplate(string id, string wwwroot);
    }
}
