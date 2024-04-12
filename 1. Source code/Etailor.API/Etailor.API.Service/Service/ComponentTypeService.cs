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
        public ComponentTypeService(ICategoryRepository categoryRepository, IComponentTypeRepository componentTypeRepository,
            IComponentRepository componentRepository)
        {
            this.categoryRepository = categoryRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.componentRepository = componentRepository;
        }
        public async Task<bool> AddComponentType(ComponentType componentType)
        {
            return false;

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
                componentType.CreatedTime = DateTime.UtcNow.AddHours(7);
                componentType.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                componentType.IsActive = true;
            });

            await Task.WhenAll(genId, checkCategory, setValue);

            return componentTypeRepository.Create(componentType);
        }
        public async Task<bool> UpdateComponentType(ComponentType componentType)
        {
            var dbComponentType = componentTypeRepository.Get(componentType.Id);
            if (dbComponentType != null && dbComponentType.IsActive == true)
            {
                var categoryComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == dbComponentType.CategoryId && x.IsActive == true);
                if (categoryComponentTypes != null && categoryComponentTypes.Any())
                {
                    categoryComponentTypes = categoryComponentTypes.ToList();

                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() =>
                    {
                        if (string.IsNullOrWhiteSpace(componentType.Name))
                        {
                            throw new UserException("Tên loại thành phần sản phẩm không được để trống");
                        }
                        else if (categoryComponentTypes.Any(x => x.Id != dbComponentType.Id && x.Name == componentType.Name))
                        {
                            throw new UserException("Tên loại thành phần sản phẩm không được trùng");
                        }
                        else
                        {
                            dbComponentType.Name = componentType.Name;
                        }
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        dbComponentType.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        dbComponentType.IsActive = true;
                    }));

                    await Task.WhenAll(tasks);

                    return componentTypeRepository.Update(dbComponentType.Id, dbComponentType);
                }
                else
                {
                    throw new UserException("Loại thành phần sản phẩm không tìm thấy");
                }
            }
            else
            {
                throw new UserException("Loại thành phần sản phẩm không tìm thấy");
            }
        }

        public async Task<List<ComponentType>> AddComponentTypes(string categoryId, List<ComponentType> componentTypes)
        {
            var tasks = new List<Task>();

            foreach (var componentType in componentTypes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var insideTasks = new List<Task>();
                    insideTasks.Add(Task.Run(() =>
                    {
                        componentType.Id = Ultils.GenGuidString();
                    }));
                    insideTasks.Add(Task.Run(() =>
                    {
                        componentType.CategoryId = categoryId;
                    }));
                    insideTasks.Add(Task.Run(() =>
                    {
                        if (string.IsNullOrWhiteSpace(componentType.Name))
                        {
                            throw new UserException("Tên loại thành phần sản phẩm không được để trống");
                        }
                        else if (componentTypes.Where(x => x.Name == componentType.Name).Count() > 1)
                        {
                            throw new UserException("Tên loại thành phần sản phẩm không được trùng");
                        }
                    }));
                    insideTasks.Add(Task.Run(() =>
                    {
                        componentType.CreatedTime = DateTime.UtcNow.AddHours(7);
                        componentType.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                        componentType.IsActive = true;
                    }));
                    await Task.WhenAll(insideTasks);
                }));
            }
            await Task.WhenAll(tasks);

            return componentTypes;
        }

        public bool DeleteComponentType(string id)
        {
            return false;

            var dbComponentType = componentTypeRepository.Get(id);
            if (dbComponentType != null)
            {
                dbComponentType.IsActive = false;
                dbComponentType.InactiveTime = DateTime.UtcNow.AddHours(7);
                dbComponentType.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);

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
            var componentTypes = componentTypeRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
            if (componentTypes != null && componentTypes.Any())
            {
                componentTypes = componentTypes.OrderBy(x => x.Name).ToList();
            }
            return componentTypes;
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
