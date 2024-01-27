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
    public class ComponentTypeService : IComponentTypeService
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IComponentRepository componentRepository;
        public ComponentTypeService(ICategoryRepository categoryRepository, IComponentTypeRepository componentTypeRepository, IComponentRepository componentRepository)
        {
            this.categoryRepository = categoryRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.componentRepository = componentRepository;
        }
        public async Task<bool> AddComponentType(ComponentType componentType)
        {
            var genId = Task.Run(() =>
            {
                componentType.Id = Ultils.GenGuidString();
            });
            var checkCategory = Task.Run(() =>
            {
                if (componentType.CategoryId == null || categoryRepository.Get(componentType.CategoryId) == null)
                {
                    throw new UserException("Loại danh mục không tìm thấy");
                }
            });
            var checkName = Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(componentType.Name))
                {
                    throw new UserException("Tên loại thành phần sản phẩm không được để trống");
                }
                else if (componentTypeRepository.GetAll(x => x.Name == componentType.Name && x.IsActive == true).Any())
                {
                    throw new UserException("Tên loại thành phần sản phẩm không được trùng");
                }
            });
            var setValue = Task.Run(() =>
            {
                componentType.CreatedTime = DateTime.Now;
                componentType.LastestUpdatedTime = DateTime.Now;
                componentType.IsActive = true;
            });

            await Task.WhenAll(genId, checkCategory, checkName, setValue);

            return componentTypeRepository.Create(componentType);
        }
        public async Task<bool> UpdateComponentType(ComponentType componentType)
        {
            var dbComponentType = componentTypeRepository.Get(componentType.Id);
            if (dbComponentType != null)
            {
                var checkCategory = Task.Run(() =>
                {
                    if (componentType.CategoryId == null || categoryRepository.Get(componentType.CategoryId) == null)
                    {
                        throw new UserException("Loại danh mục không tìm thấy");
                    }
                    else
                    {
                        dbComponentType.CategoryId = componentType.CategoryId;
                    }
                });
                var checkName = Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(componentType.Name))
                    {
                        throw new UserException("Tên loại thành phần sản phẩm không được để trống");
                    }
                    else if (componentTypeRepository.GetAll(x => x.Id != dbComponentType.Id && x.Name == componentType.Name && x.IsActive == true).Any())
                    {
                        throw new UserException("Tên loại thành phần sản phẩm không được trùng");
                    }
                    else
                    {
                        dbComponentType.Name = componentType.Name;
                    }
                });
                var setValue = Task.Run(() =>
                {
                    dbComponentType.LastestUpdatedTime = DateTime.Now;
                    dbComponentType.IsActive = true;
                });

                await Task.WhenAll(checkCategory, checkName, setValue);

                return componentTypeRepository.Update(dbComponentType.Id, dbComponentType);
            }
            else
            {
                throw new UserException("Loại thành phần sản phẩm không tìm thấy");
            }
        }

        public bool DeleteComponentType(string id)
        {
            var dbComponentType = componentTypeRepository.Get(id);
            if (dbComponentType != null)
            {
                dbComponentType.IsActive = false;
                dbComponentType.InactiveTime = DateTime.Now;
                dbComponentType.LastestUpdatedTime = DateTime.Now;

                return componentTypeRepository.Update(dbComponentType.Id, dbComponentType);
            }
            else
            {
                throw new UserException("Loại thành phần sản phẩm không tìm thấy");
            }
        }

        public ComponentType GetComponentType(string id)
        {
            var componentType = componentTypeRepository.Get(id);
            return componentType == null ? null : componentType.IsActive == true ? componentType : null;
        }

        public IEnumerable<ComponentType> GetComponentTypes(string? search)
        {
            return componentTypeRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
        }

        public IEnumerable<ComponentType> GetComponentTypesByCategory(string? id)
        {
            if (id == null || categoryRepository.Get(id) == null)
            {
                throw new UserException("Loại danh mục không tìm thấy");
            }
            else
            {
                return componentTypeRepository.GetAll(x => x.CategoryId.Trim() == id.Trim() && x.IsActive == true);
            }
        }
    }
}
