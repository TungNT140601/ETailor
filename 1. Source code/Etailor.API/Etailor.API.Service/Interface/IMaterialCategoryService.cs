using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IMaterialCategoryService
    {
        Task<bool> CreateMaterialCategory(MaterialCategory materialCategory);
        Task<bool> UpdateMaterialCategory(MaterialCategory materialCategory);
        bool DeleteMaterialCategory(string id);
        MaterialCategory GetMaterialCategory(string id);
        IEnumerable<MaterialCategory> GetMaterialCategorys(string? search);
    }
}
