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

namespace Etailor.API.Service.Service
{
    public class MaterialCategoryService
    {
        private readonly IMaterialCategoryRepository materialCategoryRepository;
        private readonly IMaterialTypeRepository materialTypeRepository;
        private readonly IMaterialRepository materialRepository;

        public MaterialCategoryService(IMaterialRepository materialRepository, IMaterialTypeRepository materialTypeRepository, IMaterialCategoryRepository materialCategoryRepository)
        {
            this.materialRepository = materialRepository;
            this.materialTypeRepository = materialTypeRepository;
            this.materialCategoryRepository = materialCategoryRepository;
        }

        public async Task<bool> CreateMaterialCategory(MaterialCategory materialCategory)
        {

            materialCategory.Id = Ultils.GenGuidString();

            var tasks = new List<Task>();
            tasks.Add(Task.Run(async () =>
            {
                if (!string.IsNullOrWhiteSpace(materialCategory.Name))
                {
                    throw new UserException("Vui lòng nhập tên");
                }
            }));
            tasks.Add(Task.Run(async () =>
            {
                if (materialCategory.PricePerUnit < 0)
                {
                    throw new UserException("Vui lòng nhập tên");
                }
            }));

            await Task.WhenAll(tasks);

            materialCategory.CreatedTime = DateTime.Now;
            materialCategory.InactiveTime = null;
            materialCategory.IsActive = true;

            return materialCategoryRepository.Create(materialCategory);
        }

        //public async Task<bool> UpdateMaterialCategory(MaterialCategory materialCategory)
        //{
        //    var existMaterialCategory = materialCategoryRepository.Get(materialCategory.Id);
        //    if (existBlog != null)
        //    {
        //        existBlog.Title = materialCategory.Name;
        //        existBlog.Content = materialCategory.PricePerUnit;

        //        var setThumbnail = Task.Run(async () =>
        //        {

        //        });
        //        await Task.WhenAll(setThumbnail);

        //        existBlog.LastestUpdatedTime = DateTime.Now;
        //        existBlog.InactiveTime = null;
        //        existBlog.IsActive = true;

        //        return blogRepository.Update(existBlog.Id, existBlog);
        //    }
        //    else
        //    {
        //        throw new UserException("Không tìm thấy bài blog này.");
        //    }
        //}

        //public bool DeleteMaterialCategory(string id)
        //{
        //    var existBlog = blogRepository.Get(id);
        //    if (existBlog != null)
        //    {
        //        existBlog.LastestUpdatedTime = DateTime.Now;
        //        existBlog.InactiveTime = DateTime.Now;
        //        existBlog.IsActive = false;
        //        return blogRepository.Update(existBlog.Id, existBlog);
        //    }
        //    else
        //    {
        //        throw new UserException("Không tìm thấy bài blog này.");
        //    }
        //}

        public async Task<MaterialCategory> GetMaterialCategory(string id)
        {
            var materialCategory = materialCategoryRepository.Get(id);

            //var setThumbnail = Task.Run(async () =>
            //{
                
            //});
            //await Task.WhenAll(setThumbnail);
            return materialCategory == null ? null : materialCategory.IsActive == true ? materialCategory : null;
        }

        //public async Task<IEnumerable<Blog>> GetMaterialCategorys(string? search)
        //{
        //    IEnumerable<Blog> ListOfBlog = blogRepository.GetAll(x => (search == null || (search != null && x.Title.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
        //    foreach (Blog blog in ListOfBlog)
        //    {
        //        var setThumbnail = Task.Run(async () =>
        //        {
                 
        //        });
        //        await Task.WhenAll(setThumbnail);
        //    };
        //    return ListOfBlog;
        //}
    }
}
