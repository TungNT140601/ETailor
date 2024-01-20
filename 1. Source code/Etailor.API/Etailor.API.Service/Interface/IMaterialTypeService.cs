using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IMaterialTypeService
    {
        bool CreateMaterialType(MaterialType materialType);
        bool UpdateMaterialType(MaterialType materialType);
        bool DeleteMaterialType(string id);
        MaterialType GetMaterialType(string id);
        IEnumerable<MaterialType> GetMaterialTypes(string? search);
    }
}
