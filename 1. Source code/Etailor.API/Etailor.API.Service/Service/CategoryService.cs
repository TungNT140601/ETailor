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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        public CategoryService(ICategoryRepository categoryRepository, IComponentTypeRepository componentTypeRepository, IProductTemplateRepository productTemplateRepository)
        {
            this.categoryRepository = categoryRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.productTemplateRepository = productTemplateRepository;
        }

        public async Task<bool> AddCategory(Category category)
        {
            var componentTypes = new List<ComponentType>();

            category.Id = Ultils.GenGuidString();

            var tasks = new List<Task>();

            var duplicateName = categoryRepository.GetAll(x => x.Name == category.Name && x.IsActive == true);

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    throw new Exception("Tên danh mục sản phẩm không được để trống");
                }
                else
                {
                    if (duplicateName.Any())
                    {
                        throw new UserException("Tên danh mục sản phầm đã được sử dụng");
                    }
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                category.CreatedTime = DateTime.Now;
                category.LastestUpdatedTime = DateTime.Now;
            }));

            if (category.ComponentTypes.Any())
            {
                componentTypes = category.ComponentTypes.ToList();

                category.ComponentTypes = null;

                foreach (var componentType in componentTypes)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        componentType.Id = Ultils.GenGuidString();
                        componentType.CreatedTime = DateTime.Now;
                        componentType.LastestUpdatedTime = DateTime.Now;
                        componentType.IsActive = true;
                        componentType.InactiveTime = null;
                        componentType.CategoryId = category.Id;
                    }));
                }
            }

            await Task.WhenAll(tasks);

            if (categoryRepository.Create(category))
            {
                return componentTypeRepository.CreateRange(componentTypes);
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategory(Category category)
        {
            var dbCategory = categoryRepository.Get(category.Id);

            if (dbCategory != null && dbCategory.IsActive == true)
            {
                var tasks = new List<Task>();

                var newComponentTypes = new List<ComponentType>();

                var activeNewComponentTypes = new List<ComponentType>();

                var disableOldComponentTypes = new List<ComponentType>();

                var duplicateName = categoryRepository.GetAll(x => dbCategory.Id != x.Id && x.Name == category.Name && x.IsActive == true);

                var oldComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == dbCategory.Id && x.IsActive == true).ToList();

                tasks.Add(Task.Run(() =>
                {
                    if (duplicateName.Any())
                    {
                        throw new UserException("Tên danh mục sản phầm đã được sử dụng");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    dbCategory.LastestUpdatedTime = DateTime.Now;
                }));

                if (category.ComponentTypes.Any())
                {
                    tasks.Add(Task.Run(() =>
                    {
                        newComponentTypes = category.ComponentTypes.ToList();
                    }));

                    await Task.WhenAll(tasks);

                    if (oldComponentTypes.Any() && oldComponentTypes.Count > 0)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            activeNewComponentTypes = newComponentTypes.Where(x => !oldComponentTypes.Select(c => c.Name).ToList().Contains(x.Name)).ToList();
                        }));
                        tasks.Add(Task.Run(() =>
                        {
                            disableOldComponentTypes = oldComponentTypes.Where(x => !newComponentTypes.Select(c => c.Name).ToList().Contains(x.Name)).ToList();
                        }));
                    }
                    else
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            activeNewComponentTypes = newComponentTypes;
                        }));
                        tasks.Add(Task.Run(() =>
                        {
                            disableOldComponentTypes = oldComponentTypes;
                        }));
                    }

                    await Task.WhenAll(tasks);

                    foreach (var componentType in activeNewComponentTypes)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            componentType.Id = Ultils.GenGuidString();
                            componentType.CreatedTime = DateTime.Now;
                            componentType.LastestUpdatedTime = DateTime.Now;
                            componentType.IsActive = true;
                            componentType.InactiveTime = null;
                            componentType.CategoryId = dbCategory.Id;
                        }));
                    }

                    foreach (var componentType in disableOldComponentTypes)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            componentType.IsActive = false;
                            componentType.InactiveTime = DateTime.Now;
                        }));
                    }
                }

                await Task.WhenAll(tasks);

                if (categoryRepository.Update(dbCategory.Id, dbCategory))
                {
                    var check = new List<bool>();
                    if (disableOldComponentTypes != null)
                    {
                        foreach (var componentType in disableOldComponentTypes)
                        {
                            check.Add(componentTypeRepository.Update(componentType.Id, componentType));
                        }
                        if (check.Any(x => x == false))
                        {
                            throw new SystemsException("Lỗi trong quá trình tạo mới bộ phận của loại bản mẫu");
                        }
                    }
                    return componentTypeRepository.CreateRange(activeNewComponentTypes);
                }
                else
                {
                    throw new SystemsException("Lỗi trong quá trình cập nhật");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy danh mục sản phầm");
            }
        }

        public async Task<bool> DeleteCategory(string id)
        {
            var dbCategory = categoryRepository.Get(id);
            if (dbCategory != null && dbCategory.IsActive == true)
            {
                var checkChild = Task.Run(() =>
                {
                    if (productTemplateRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any() || componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
                    {
                        throw new UserException("Không thể xóa danh mục sản phầm này do vẫn còn các mẫu sản phẩm và các loại thành phần sản phẩm vẫn còn thuộc danh mục này");
                    }
                });
                var setValue = Task.Run(() =>
                {
                    dbCategory.LastestUpdatedTime = DateTime.Now;
                    dbCategory.IsActive = false;
                    dbCategory.InactiveTime = DateTime.Now;
                });

                await Task.WhenAll(checkChild, setValue);

                return categoryRepository.Update(dbCategory.Id, dbCategory);
            }
            else
            {
                throw new UserException("Không tìm thấy danh mục sản phầm");
            }
        }

        public Category GetCategory(string id)
        {
            var category = categoryRepository.Get(id);
            if (category != null && category.IsActive == true)
            {
                category.ComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).ToList();
                return category;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Category> GetCategorys(string? search)
        {
            return categoryRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
