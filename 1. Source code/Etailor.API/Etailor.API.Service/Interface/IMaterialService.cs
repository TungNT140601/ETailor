using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IMaterialService
    {
        Task<bool> AddMaterial(Material material);

        Task<bool> UpdateMaterial(Material bodyAttribute);

        Task<bool> DeleteMaterial(string id);

        Material GetMaterial(string id);

        IEnumerable<Material> GetMaterialsByMaterialCategory(string? search);
        IEnumerable<Material> GetMaterials(string? search);
    }
}
