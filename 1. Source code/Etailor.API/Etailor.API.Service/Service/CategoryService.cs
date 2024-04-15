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
        private readonly IComponentTypeService componentTypeService;
        private readonly IComponentTypeRepository componentTypeRepository;
        private readonly IProductTemplateRepository productTemplateRepository;
        public CategoryService(ICategoryRepository categoryRepository, IComponentTypeRepository componentTypeRepository, IProductTemplateRepository productTemplateRepository
            , IComponentTypeService componentTypeService)
        {
            this.categoryRepository = categoryRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.productTemplateRepository = productTemplateRepository;
            this.componentTypeService = componentTypeService;
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
                category.CreatedTime = DateTime.UtcNow.AddHours(7);
                category.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
            }));

            await Task.WhenAll(tasks);

            if (category.ComponentTypes != null && category.ComponentTypes.Any())
            {
                category.ComponentTypes = await componentTypeService.AddComponentTypes(category.Id, category.ComponentTypes.ToList());
            }
            else
            {
                throw new UserException("Cần phải có ít nhất một bộ phận của danh mục");
            }

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
                var dbCategoryComponentTypes = componentTypeRepository.GetAll(x => x.CategoryId == dbCategory.Id && x.IsActive == true);
                if (dbCategoryComponentTypes != null && dbCategoryComponentTypes.Any())
                {
                    dbCategoryComponentTypes = dbCategoryComponentTypes.ToList();
                    var tasks = new List<Task>();

                    var duplicateName = categoryRepository.GetAll(x => dbCategory.Id != x.Id && x.Name == category.Name && x.IsActive == true);

                    tasks.Add(Task.Run(() =>
                    {
                        if (duplicateName.Any())
                        {
                            throw new UserException("Tên danh mục sản phầm đã được sử dụng");
                        }
                        else
                        {
                            dbCategory.Name = category.Name;
                        }
                    }));

                    tasks.Add(Task.Run(() =>
                    {
                        dbCategory.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                        dbCategory.IsActive = true;
                        dbCategory.InactiveTime = null;
                    }));

                    tasks.Add(Task.Run(async () =>
                    {
                        if (dbCategory.ComponentTypes == null || dbCategory.ComponentTypes == new List<ComponentType>())
                        {
                            dbCategory.ComponentTypes = dbCategoryComponentTypes.ToList();
                        }
                        var insideTasks = new List<Task>();
                        foreach (var dbComponentType in dbCategory.ComponentTypes)
                        {
                            insideTasks.Add(Task.Run(() =>
                            {
                                var componentType = category.ComponentTypes.FirstOrDefault(x => x.Id == dbComponentType.Id);
                                if (componentType != null)
                                {
                                    if (!dbComponentType.Name.Equals(componentType.Name))
                                    {
                                        dbComponentType.Name = componentType.Name;
                                        dbComponentType.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                                    }
                                }
                            }));
                        }
                        await Task.WhenAll(insideTasks);
                    }));

                    await Task.WhenAll(tasks);

                    if (categoryRepository.Update(dbCategory.Id, dbCategory))
                    {
                        return true;
                    }
                    else
                    {
                        throw new SystemsException("Lỗi trong quá trình cập nhật", nameof(CategoryService));
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy các loại bộ phận của danh mục");
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
                    if (productTemplateRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
                    {
                        throw new UserException("Không thể xóa danh mục sản phầm này do vẫn còn các mẫu sản phẩm và các loại thành phần sản phẩm vẫn còn thuộc danh mục này");
                    }
                });
                var setValue = Task.Run(() =>
                {
                    dbCategory.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    dbCategory.IsActive = false;
                    dbCategory.InactiveTime = DateTime.UtcNow.AddHours(7);
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

        public async Task<IEnumerable<Category>> GetCategorys(string? search)
        {
            var categories = categoryRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
            if (categories != null && categories.Any())
            {
                categories = categories.ToList();
                var categoriesComponentTypes = componentTypeRepository.GetAll(x => categories.Select(c => c.Id).Contains(x.CategoryId) && x.IsActive == true);
                if (categoriesComponentTypes != null && categoriesComponentTypes.Any())
                {
                    categoriesComponentTypes = categoriesComponentTypes.ToList();
                    var tasks = new List<Task>();
                    foreach (var category in categories)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            category.ComponentTypes = categoriesComponentTypes.Where(x => x.CategoryId == category.Id).ToList();
                        }));
                    }
                    await Task.WhenAll(tasks);
                }
            }
            return categories;
        }


    }
}
