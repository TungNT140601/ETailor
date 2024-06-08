using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IMaterialService
    {
        Task<bool> AddMaterial(Material material, IFormFile? image, string wwwroot);

        Task<bool> UpdateMaterial(Material material, IFormFile? image, string wwwroot);

        bool DeleteMaterial(string id);

        Material GetMaterial(string id);

        IEnumerable<Material> GetMaterialsByMaterialCategory(string materialCategoryId);
        IEnumerable<Material> GetMaterials(string? search);
        Task<IEnumerable<Material>> GetFabricMaterials(string? search);
    }
}
