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

namespace Etailor.API.Service.Service
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository materialRepository;

        public MaterialService (IMaterialRepository materialRepository)
        {
            this.materialRepository = materialRepository;
        }

        public async Task<bool> AddMaterial(Material material)
        {
            var checkDuplicateId = Task.Run(() =>
            {
                if (materialRepository.GetAll(x => x.Id == material.Id && x.IsActive == true).Any())
                {
                    throw new UserException("Mã Id nguyên liệu này đã được sử dụng");
                }
            });
            var setValue = Task.Run(() =>
            {
                material.Id = Ultils.GenGuidString();
                material.CreatedTime = DateTime.Now;
                material.LastestUpdatedTime = DateTime.Now;
                material.InactiveTime = null;
                material.IsActive = true;
            });

            await Task.WhenAll(checkDuplicateId, setValue);

            return materialRepository.Create(material);
        }

        public async Task<bool> UpdateMaterial(Material material)
        {
            var dbMaterial = materialRepository.Get(material.Id);
            if (dbMaterial != null && dbMaterial.IsActive == true)
            {
                var setValue = Task.Run(() =>
                {
                    dbMaterial.Name = material.Name;
                    dbMaterial.Image = material.Image;
                    dbMaterial.Quantity = material.Quantity;

                    dbMaterial.CreatedTime = null;
                    dbMaterial.LastestUpdatedTime = DateTime.Now;
                    dbMaterial.InactiveTime = null;
                    dbMaterial.IsActive = true;
                });

                await Task.WhenAll(setValue);

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
                var checkChild = Task.Run(() =>
                {
                    //if (productTemplateRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any() || componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
                    //{
                    //    throw new UserException("Không thể xóa danh mục sản phầm này do vẫn còn các mẫu sản phẩm và các loại thành phần sản phẩm vẫn còn thuộc danh mục này");
                    //}
                });
                var setValue = Task.Run(() =>
                {
                    dbMaterial.CreatedTime = null;
                    dbMaterial.LastestUpdatedTime = DateTime.Now;
                    dbMaterial.IsActive = false;
                    dbMaterial.InactiveTime = DateTime.Now;
                });

                await Task.WhenAll(checkChild, setValue);

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
            return material == null ? null : material.IsActive == true ? material : null;
        }

        public IEnumerable<Material> GetMaterialsByMaterialCategory(string? search)
        {
            return materialRepository.GetAll(x => ((search != null && x.MaterialCategoryId.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
