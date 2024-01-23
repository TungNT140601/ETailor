using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CustomException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class MaterialTypeService : IMaterialTypeService
    {
        private readonly IMaterialTypeRepository materialTypeRepository;
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        public MaterialTypeService(IMaterialTypeRepository materialTypeRepository, IMaterialCategoryRepository materialCategoryRepository)
        {
            this.materialTypeRepository = materialTypeRepository;
            this.materialCategoryRepository = materialCategoryRepository;
        }
        private bool CheckNameDup(string? id, string name)
        {
            return materialTypeRepository.GetAll(x => (id == null || (id != null && x.Id != id)) && (x.Name != null && x.Name.Trim().ToLower() == name.Trim().ToLower()) && x.IsActive == true).Any();
        }

        public bool CreateMaterialType(MaterialType materialType)
        {
            materialType.Id = Ultils.GenGuidString();
            if (CheckNameDup(null, materialType.Name))
            {
                throw new UserException("Tên loại nguyên liệu đã được sử dụng");
            }
            materialType.LastestUpdatedTime = DateTime.Now;
            materialType.CreatedTime = DateTime.Now;
            materialType.InactiveTime = null;
            materialType.IsActive = true;

            return materialTypeRepository.Create(materialType);
        }

        public bool UpdateMaterialType(MaterialType materialType)
        {
            var existMaterial = materialTypeRepository.Get(materialType.Id);
            if (existMaterial != null)
            {
                if (CheckNameDup(existMaterial.Id, materialType.Name))
                {
                    throw new UserException("Tên loại nguyên liệu đã được sử dụng");
                }
                else
                {
                    existMaterial.Name = materialType.Name;
                }
                existMaterial.LastestUpdatedTime = DateTime.Now;
                existMaterial.InactiveTime = null;
                existMaterial.IsActive = true;

                return materialTypeRepository.Update(existMaterial.Id, existMaterial);
            }
            else
            {
                throw new UserException("Không tìm thấy loại nguyên liệu");
            }
        }

        public bool DeleteMaterialType(string id)
        {
            var existMaterial = materialTypeRepository.Get(id);
            if (existMaterial != null)
            {
                var materialCates = materialCategoryRepository.GetAll(x => x.MaterialTypeId == id && x.IsActive == true);
                if (materialCates.Any())
                {
                    throw new UserException("Loại nguyên liệu này vẫn còn trong cửa hàng, vui lòng kiểm tra lại trước khi xóa.");
                }
                else
                {
                    existMaterial.LastestUpdatedTime = DateTime.Now;
                    existMaterial.InactiveTime = DateTime.Now;
                    existMaterial.IsActive = false;
                    return materialTypeRepository.Update(existMaterial.Id, existMaterial);
                }
            }
            else
            {
                throw new UserException("Không tìm thấy loại nguyên liệu");
            }
        }

        public MaterialType GetMaterialType(string id)
        {
            return materialTypeRepository.Get(id);
        }

        public IEnumerable<MaterialType> GetMaterialTypes(string? search)
        {
            return materialTypeRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
        }
    }
}
