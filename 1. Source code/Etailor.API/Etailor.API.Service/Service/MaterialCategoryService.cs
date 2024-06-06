using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Etailor.API.Service.Interface;

namespace Etailor.API.Service.Service
{
    public class MaterialCategoryService : IMaterialCategoryService
    {
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        private readonly IMaterialRepository materialRepository;

        public MaterialCategoryService(IMaterialRepository materialRepository, IMaterialCategoryRepository materialCategoryRepository)
        {
            this.materialRepository = materialRepository;
            this.materialCategoryRepository = materialCategoryRepository;
        }

        public async Task<bool> CreateMaterialCategory(MaterialCategory materialCategory)
        {

            materialCategory.Id = Ultils.GenGuidString();

            var tasks = new List<Task>();

            tasks.Add(Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(materialCategory.Name))
                {
                    throw new UserException("Vui lòng nhập tên");
                }
            }));

            await Task.WhenAll(tasks);

            materialCategory.CreatedTime = DateTime.UtcNow.AddHours(7);
            materialCategory.InactiveTime = null;
            materialCategory.IsActive = true;

            return materialCategoryRepository.Create(materialCategory);
        }

        public async Task<bool> UpdateMaterialCategory(MaterialCategory materialCategory)
        {
            var existMaterialCategory = materialCategoryRepository.Get(materialCategory.Id);
            if (existMaterialCategory != null)
            {
                existMaterialCategory.Name = materialCategory.Name;

                var tasks = new List<Task>();

                tasks.Add(Task.Run(async () =>
                {
                    if (string.IsNullOrWhiteSpace(materialCategory.Name))
                    {
                        throw new UserException("Vui lòng nhập tên");
                    }
                }));

                await Task.WhenAll(tasks);

                return materialCategoryRepository.Update(existMaterialCategory.Id, existMaterialCategory);
            }
            else
            {
                throw new UserException("Không tìm thấy danh muc nguyen lieu này.");
            }
        }

        public bool DeleteMaterialCategory(string id)
        {
            var existMaterialCategory = materialCategoryRepository.Get(id);
            if (existMaterialCategory != null)
            {
                existMaterialCategory.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                existMaterialCategory.InactiveTime = DateTime.UtcNow.AddHours(7);
                existMaterialCategory.IsActive = false;
                return materialCategoryRepository.Update(existMaterialCategory.Id, existMaterialCategory);
            }
            else
            {
                throw new UserException("Không tìm thấy danh muc nguyen lieu này.");
            }
        }

        public MaterialCategory GetMaterialCategory(string id)
        {
            var materialCategory = materialCategoryRepository.Get(id);

            return materialCategory == null ? null : materialCategory.IsActive == true ? materialCategory : null;
        }

        public IEnumerable<MaterialCategory> GetMaterialCategorys(string? search)
        {
            IEnumerable<MaterialCategory> ListOfMaterialCategory = materialCategoryRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
            return ListOfMaterialCategory;
        }
    }
}
