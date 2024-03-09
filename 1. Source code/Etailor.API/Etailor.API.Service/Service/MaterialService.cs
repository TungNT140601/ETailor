using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Etailor.API.Service.Service
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository materialRepository;
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        private readonly IMaterialTypeRepository materialTypeRepository;

        public MaterialService(IMaterialRepository materialRepository, IMaterialCategoryRepository materialCategoryRepository, IMaterialTypeRepository materialTypeRepository)
        {
            this.materialRepository = materialRepository;
            this.materialCategoryRepository = materialCategoryRepository;
            this.materialTypeRepository = materialTypeRepository;
        }

        public async Task<bool> AddMaterial(Material material, IFormFile? image, string wwwroot)
        {
            var tasks = new List<Task>();

            var materialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(material.Name))
                {
                    throw new UserException("Tên nguyên liệu không được để trống");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (materialCategory == null || materialCategory.IsActive == false)
                {
                    throw new UserException("Không tìm thấy loại nguyện liệu");
                }
            }));

            tasks.Add(Task.Run(async () =>
            {
                if (image != null)
                {
                    material.Image = await Ultils.UploadImage(wwwroot, "Materials", image, null);
                }
                else
                {
                    material.Image = "";
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                material.Id = Ultils.GenGuidString();
            }));

            tasks.Add(Task.Run(() =>
            {
                if (!material.Quantity.HasValue || material.Quantity.Value < 0)
                {
                    throw new UserException("Số lượng không hợp lệ");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                material.CreatedTime = DateTime.Now;
                material.LastestUpdatedTime = DateTime.Now;
                material.InactiveTime = null;
                material.IsActive = true;
            }));

            await Task.WhenAll(tasks);

            return materialRepository.Create(material);
        }

        public async Task<bool> UpdateMaterial(Material material, IFormFile? image, string wwwroot)
        {
            var dbMaterial = materialRepository.Get(material.Id);
            if (dbMaterial != null && dbMaterial.IsActive == true)
            {
                var tasks = new List<Task>();

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(material.Name))
                    {
                        throw new UserException("Tên nguyên liệu không được để trống");
                    }
                    else
                    {
                        dbMaterial.Name = material.Name;
                    }
                }));

                tasks.Add(Task.Run(async () =>
                {
                    if (image != null && !string.IsNullOrWhiteSpace(dbMaterial.Image))
                    {
                        dbMaterial.Image = await Ultils.UploadImage(wwwroot, "Materials", image, dbMaterial.Image);
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!material.Quantity.HasValue || material.Quantity.Value < 0)
                    {
                        throw new UserException("Số lượng không hợp lệ");
                    }
                    else
                    {
                        dbMaterial.Quantity = material.Quantity;
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbMaterial.LastestUpdatedTime = DateTime.Now;
                    dbMaterial.InactiveTime = null;
                    dbMaterial.IsActive = true;
                }));

                await Task.WhenAll(tasks);

                return materialRepository.Update(dbMaterial.Id, dbMaterial);
            }
            else
            {
                throw new UserException("Không tìm thấy nguyên liệu này");
            }
        }

        public async Task<bool> DeleteMaterial(string id)
        {
            var dbMaterial = materialRepository.Get(id);
            if (dbMaterial != null && dbMaterial.IsActive == true)
            {
                var setValue = Task.Run(() =>
                {
                    dbMaterial.LastestUpdatedTime = DateTime.Now;
                    dbMaterial.IsActive = false;
                    dbMaterial.InactiveTime = DateTime.Now;
                });

                await Task.WhenAll(setValue);

                return materialRepository.Update(dbMaterial.Id, dbMaterial);
            }
            else
            {
                throw new UserException("Không tìm thấy nguyên liệu nầy");
            }
        }

        public Material GetMaterial(string id)
        {
            var material = materialRepository.Get(id);
            if (material != null && material.MaterialCategory == null && !string.IsNullOrEmpty(material.MaterialCategoryId))
            {
                material.MaterialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);
            }
            return material == null ? null : material.IsActive == true ? material : null;
        }

        public IEnumerable<Material> GetMaterialsByMaterialCategory(string? materialCategoryId)
        {
            var materialCategory = materialCategoryRepository.Get(materialCategoryId);
            if (materialCategory != null && materialCategory.IsActive == true)
            {
                var materials = materialRepository.GetAll(x => x.MaterialCategoryId == materialCategory.Id && x.IsActive == true);
                if (materials != null && materials.Any())
                {
                    materials = materials.ToList();
                    foreach (var material in materials)
                    {
                        if (material.MaterialCategory == null)
                        {
                            material.MaterialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);
                        }
                    }
                }

                return materials;
            }
            else
            {
                return new List<Material>();
            }
        }

        public IEnumerable<Material> GetMaterials(string? search)
        {
            var materials = materialRepository.GetAll(x => (string.IsNullOrWhiteSpace(search) || (search != null && x.Name.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
            if (materials != null && materials.Any())
            {
                materials = materials.ToList();
                foreach (var material in materials)
                {
                    if (material.MaterialCategory == null)
                    {
                        material.MaterialCategory = materialCategoryRepository.Get(material.MaterialCategoryId);
                    }
                }
            }

            return materials;
        }

        public IEnumerable<Material> GetMaterialsByMaterialType(string materialTypeId)
        {
            var materialType = materialTypeRepository.Get(materialTypeId);
            if (materialType != null && materialType.IsActive == true)
            {
                var materialCategories = materialCategoryRepository.GetAll(x => x.MaterialTypeId == materialType.Id && x.IsActive == true);

                if (materialCategories.Any() && materialCategories.Count() > 0)
                {
                    return materialRepository.GetAll(x => materialCategories.Select(m => m.Id).Contains(x.MaterialCategoryId) && x.IsActive == true);
                }
            }
            return new List<Material>();
        }

        public IEnumerable<Material> GetFabricMaterials(string? search)
        {
            var materialTypes = materialTypeRepository.GetAll(x => (x.Name.Trim().ToLower().Contains("vai") || x.Name.Trim().ToLower().Contains("vải") || x.Name.Trim().ToLower().Contains("vãi")) && x.IsActive == true).ToList();
            if (materialTypes.Any() && materialTypes.Count > 0)
            {
                var materialCategories = materialCategoryRepository.GetAll(x => materialTypes.Select(m => m.Id).Contains(x.MaterialTypeId) && x.IsActive == true).ToList();

                if (materialCategories.Any() && materialCategories.Count > 0)
                {
                    return materialRepository.GetAll(x => (string.IsNullOrWhiteSpace(search) || x.Name.Trim().ToLower().Contains(search.Trim().ToLower())) && materialCategories.Select(m => m.Id).Contains(x.MaterialCategoryId) && x.IsActive == true);
                }
            }
            return new List<Material>();
        }
    }
}
