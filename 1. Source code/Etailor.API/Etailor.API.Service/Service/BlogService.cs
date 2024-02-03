using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Etailor.API.Repository.Interface;
using System.Security.Claims;

namespace Etailor.API.Service.Service
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository blogRepository;

        public BlogService(IBlogRepository blogRepository)
        {
            this.blogRepository = blogRepository;
        }

        public bool CreateBlog(Blog blog)
        {
            blog.Id = Ultils.GenGuidString();
            blog.UrlPath = Ultils.ConvertToEnglishAlphabet(blog.Title);

            blog.LastestUpdatedTime = DateTime.Now;
            blog.CreatedTime = DateTime.Now;
            blog.InactiveTime = null;
            blog.IsActive = true;
            return blogRepository.Create(blog);
        }

        public bool UpdateBlog(Blog blog)
        {
            var existBlog = blogRepository.Get(blog.Id);
            if (existBlog != null)
            {
                existBlog.Title = blog.Title;
                existBlog.Content = blog.Content;
                existBlog.UrlPath = Ultils.ConvertToEnglishAlphabet(blog.Title);

                existBlog.LastestUpdatedTime = DateTime.Now;
                existBlog.InactiveTime = null;
                existBlog.IsActive = true;

                return blogRepository.Update(existBlog.Id, existBlog);
            }
            else
            {
                throw new UserException("Không tìm thấy bài blog này.");
            }
        }

        public bool DeleteBlog(string id)
        {
            var existBlog = blogRepository.Get(id);
            if (existBlog != null)
            {
                existBlog.LastestUpdatedTime = DateTime.Now;
                existBlog.InactiveTime = DateTime.Now;
                existBlog.IsActive = false;
                return blogRepository.Update(existBlog.Id, existBlog);
            }
            else
            {
                throw new UserException("Không tìm thấy bài blog này.");
            }
        }

        public Blog GetBlog(string id)
        {
            var bodySize = blogRepository.Get(id);
            return bodySize == null ? null : bodySize.IsActive == true ? bodySize : null;
        }

        public IEnumerable<Blog> GetBlogs(string? search)
        {
            return blogRepository.GetAll(x => (search == null || (search != null && x.Title.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
        }

    }
}
