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
using System.Xml.Linq;

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

        public async Task<bool> CreateMaterialType(MaterialType materialType)
        {
            var check = false;
            if (!string.IsNullOrWhiteSpace(materialType.Name))
            {
                var duplicateName = materialTypeRepository.GetAll(x => (x.Name != null && x.Name.Trim().ToLower() == materialType.Name.Trim().ToLower()) && x.IsActive == true);
                check = duplicateName == null || !duplicateName.Any();
            }

            var tasks = new List<Task>();

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(materialType.Name))
                {
                    throw new UserException("Tên loại nguyên liệu không được để trống");
                }
                else if (check)
                {
                    throw new UserException("Tên loại nguyên liệu đã được sử dụng");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(materialType.Unit))
                {
                    throw new UserException("Đơn vị tính của loại nguyên liệu không được để trống");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                materialType.Id = Ultils.GenGuidString();
            }));

            tasks.Add(Task.Run(() =>
            {
                materialType.LastestUpdatedTime = DateTime.Now;
                materialType.CreatedTime = DateTime.Now;
            }));

            tasks.Add(Task.Run(() =>
            {
                materialType.InactiveTime = null;
                materialType.IsActive = true;
            }));

            await Task.WhenAll(tasks);

            return materialTypeRepository.Create(materialType);
        }

        public async Task<bool> UpdateMaterialType(MaterialType materialType)
        {
            var dbMaterialType = materialTypeRepository.Get(materialType.Id);
            if (dbMaterialType != null)
            {
                var check = false;
                if (!string.IsNullOrWhiteSpace(materialType.Name))
                {
                    var duplicateName = materialTypeRepository.GetAll(x => x.Id != dbMaterialType.Id && x.Name != null && x.Name.Trim().ToLower() == materialType.Name.Trim().ToLower() && x.IsActive == true);
                    check = duplicateName == null || !duplicateName.Any();
                }
                var tasks = new List<Task>();

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(materialType.Name))
                    {
                        throw new UserException("Tên loại nguyên liệu không được để trống");
                    }
                    else if (check)
                    {
                        throw new UserException("Tên loại nguyên liệu đã được sử dụng");
                    }
                    else
                    {
                        dbMaterialType.Name = materialType.Name;
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(materialType.Name))
                    {
                        throw new UserException("Tên loại nguyên liệu không được để trống");
                    }
                    else
                    {
                        dbMaterialType.Unit = materialType.Unit;
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbMaterialType.LastestUpdatedTime = DateTime.Now;
                    dbMaterialType.InactiveTime = null;
                    dbMaterialType.IsActive = true;
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(materialType.Unit))
                    {
                        throw new UserException("Đơn vị tính của loại nguyên liệu không được để trống");
                    }
                }));

                await Task.WhenAll(tasks);

                return materialTypeRepository.Update(dbMaterialType.Id, dbMaterialType);
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
                if (materialCates.Any() && materialCates.Count() > 0)
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
