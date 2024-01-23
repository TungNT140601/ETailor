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

        public bool AddCategory(Category category)
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
                else
                {
                    category.Id = Ultils.GenGuidString();
                    category.CreatedTime = DateTime.Now;
                    category.LastestUpdatedTime = DateTime.Now;

                    return categoryRepository.Create(category);
                }
            }
        }

        public bool UpdateCategory(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new Exception("Tên danh mục sản phẩm không được để trống");
            }
            else
            {
                var dbCategory = categoryRepository.Get(category.Id);
                if (dbCategory != null && dbCategory.IsActive == true)
                {
                    if (categoryRepository.GetAll(x => dbCategory.Id != x.Id && x.Name == category.Name && x.IsActive == true).Any())
                    {
                        throw new UserException("Tên danh mục sản phầm đã được sử dụng");
                    }
                    else
                    {
                        dbCategory.Name = category.Name;
                        dbCategory.LastestUpdatedTime = DateTime.Now;

                        return categoryRepository.Update(category.Id, category);
                    }
                }
                else
                {
                    throw new UserException("Không tìm thấy danh mục sản phầm");
                }
            }
        }

        public bool DeleteCategory(string id)
        {
            var dbCategory = categoryRepository.Get(id);
            if (dbCategory != null && dbCategory.IsActive == true)
            {
                if (catalogRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any() || componentTypeRepository.GetAll(x => x.CategoryId == id && x.IsActive == true).Any())
                {
                    throw new UserException("Không thể xóa danh mục sản phầm này do vẫn còn các mẫu sản phẩm và các loại thành phần sản phẩm vẫn còn thuộc danh mục này");
                }
                else
                {
                    dbCategory.LastestUpdatedTime = DateTime.Now;
                    dbCategory.IsActive = false;
                    dbCategory.InactiveTime = DateTime.Now;

                    return categoryRepository.Update(dbCategory.Id, dbCategory);
                }
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
