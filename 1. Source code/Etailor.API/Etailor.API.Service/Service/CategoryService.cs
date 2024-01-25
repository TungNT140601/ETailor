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
        private readonly ICatalogRepository catalogRepository;
        public CategoryService(ICategoryRepository categoryRepository, IComponentTypeRepository componentTypeRepository, ICatalogRepository catalogRepository)
        {
            this.categoryRepository = categoryRepository;
            this.componentTypeRepository = componentTypeRepository;
            this.catalogRepository = catalogRepository;
        }

        public async Task<bool> AddCategory(Category category)
        {
            var checkName = Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    throw new Exception("Tên danh mục sản phẩm không được để trống");
                }
                else
                {
                    if (categoryRepository.GetAll(x => x.Name == category.Name && x.IsActive == true).Any())
                    {
                        throw new UserException("Tên danh mục sản phầm đã được sử dụng");
                    }
                }
            });
            var setValue = Task.Run(() =>
            {
                category.Id = Ultils.GenGuidString();
                category.CreatedTime = DateTime.Now;
                category.LastestUpdatedTime = DateTime.Now;
            });

            await Task.WhenAll(checkName, setValue);

            return categoryRepository.Create(category);
        }

        public async Task<bool> UpdateCategory(Category category)
        {
            var dbCategory = categoryRepository.Get(category.Id);
            if (dbCategory != null && dbCategory.IsActive == true)
            {
                var checkName = Task.Run(() =>
                {
                    if (categoryRepository.GetAll(x => dbCategory.Id != x.Id && x.Name == category.Name && x.IsActive == true).Any())
                    {
                        throw new UserException("Tên danh mục sản phầm đã được sử dụng");
                    }
                });

                var setValue = Task.Run(() =>
                {
                    dbCategory.Name = category.Name;
                    dbCategory.LastestUpdatedTime = DateTime.Now;
                });

                await Task.WhenAll(checkName, setValue);

                return categoryRepository.Update(dbCategory.Id, dbCategory);
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
                    if (catalogRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any() || componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
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
            return category == null ? null : category.IsActive == true ? category : null;
        }

        public IEnumerable<Category> GetCategorys(string? search)
        {
            return categoryRepository.GetAll(x => (search == null || (search != null && x.Name.Trim().ToLower().Contains(search.ToLower().Trim()))) && x.IsActive == true);
        }
    }
}
