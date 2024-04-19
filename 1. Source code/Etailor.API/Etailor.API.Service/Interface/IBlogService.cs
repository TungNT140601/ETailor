using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IBlogService
    {
        Task<bool> CreateBlog(Blog blog, string wwwroot, IFormFile? avatar);

        Task<bool> UpdateBlog(Blog blog, string wwwroot, IFormFile? avatar);

        bool DeleteBlog(string id);

        Task<Blog> GetBlog(string id);
        Task<Blog> GetBlogUrl(string urlPath);

        Task<IEnumerable<Blog>> GetBlogs(string? search);

        Task<IEnumerable<Blog>> GetRelativeBlog(string? hastag);
    }
}
