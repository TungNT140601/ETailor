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
using Microsoft.AspNetCore.Http;
using System.Collections;

namespace Etailor.API.Service.Service
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository blogRepository;

        public BlogService(IBlogRepository blogRepository)
        {
            this.blogRepository = blogRepository;
        }

        public async Task<bool> CreateBlog(Blog blog, string wwwroot, IFormFile? thumbnail)
        {
            blog.Id = Ultils.GenGuidString();
            blog.UrlPath = Ultils.ConvertToEnglishAlphabet(blog.Title);

            var tasks = new List<Task>();

            tasks.Add(Task.Run(async () =>
            {
                if (blog.Title == null !| blog.Title == "undefined")
                {
                    throw new UserException("Vui lòng nhập tên tiêu đề"); 
                }
            }));

            tasks.Add(Task.Run(async () =>
            {
                if (blog.Title == null! | blog.Title == "undefined")
                {
                    throw new UserException("Vui lòng nhập tên tiêu đề");
                }
            }));

            tasks.Add(Task.Run(async () =>
            {
                if (blog.Content == null! | blog.Content == "undefined")
                {
                    throw new UserException("Vui lòng điền nội dung bài viết");
                }
            }));

            tasks.Add(Task.Run(async ()  =>
            {
                if (thumbnail != null)
                {
                    blog.Thumbnail = await Ultils.UploadImage(wwwroot, "Thumbnail", thumbnail, null);
                }
                else
                {
                    blog.Thumbnail = string.Empty;
                    //blog.Thumbnail = await Ultils.GetUrlImage("https://drive.google.com/file/d/100YI-uovn5PdEhn4IeB1RYj4B41kcFIi/view");
                }
            }));

            
            await Task.WhenAll(tasks);

            blog.LastestUpdatedTime = DateTime.Now;
            blog.CreatedTime = DateTime.Now;
            blog.InactiveTime = null;
            blog.IsActive = true;
            return blogRepository.Create(blog);
        }

        public async Task<bool> UpdateBlog(Blog blog, string wwwroot, IFormFile? thumbnail)
        {
            var existBlog = blogRepository.Get(blog.Id);
            if (existBlog != null)
            {
               
               
                existBlog.UrlPath = Ultils.ConvertToEnglishAlphabet(blog.Title);
                existBlog.StaffId = blog.StaffId;
                existBlog.Hastag = blog.Hastag;

                var setThumbnail = Task.Run(async () =>
                {
                    if (thumbnail != null)
                    {
                        existBlog.Thumbnail = await Ultils.UploadImage(wwwroot, "Thumbnail", thumbnail, null);
                    }
                });

                var tasks = new List<Task>();
                tasks.Add(Task.Run(async () =>
                {
                    if (blog.Title == null! | blog.Title == "undefined")
                    {
                        throw new UserException("Vui lòng nhập tên tiêu đề");
                    }
                    else
                    {
                        existBlog.Title = blog.Title;
                    }
                }));

                tasks.Add(Task.Run(async () =>
                {
                    if (blog.Content == null! | blog.Content == "undefined")
                    {
                        throw new UserException("Vui lòng điền nội dung bài viết");
                    }
                    else
                    {
                        existBlog.Content = blog.Content;
                    }
                }));

                var setHashtag = Task.Run(async () =>
                {
                    if (blog.Hastag != null )
                    {
                        existBlog.Hastag = blog.Hastag;
                    }
                    else
                    {
                        throw new UserException("Vui lòng không để trống Hashteag");
                    }
                });            

                await Task.WhenAll(setThumbnail);
                await Task.WhenAll(setHashtag);

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

        public async Task<Blog> GetBlog(string id)
        {
            var blog = blogRepository.Get(id);

            var setThumbnail = Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(blog.Thumbnail))
                {
                    blog.Thumbnail = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                }
                else
                {
                    blog.Thumbnail = await Ultils.GetUrlImage(blog.Thumbnail);
                }
            });
            await Task.WhenAll(setThumbnail);

            return blog == null ? null : blog.IsActive == true ? blog : null;
        }

        public async Task<IEnumerable<Blog>> GetBlogs(string? search)
        {
            IEnumerable<Blog> ListOfBlog = blogRepository.GetAll(x => (search == null || (search != null && x.Title.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
            foreach (Blog blog in ListOfBlog)
            {
                var setThumbnail = Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(blog.Thumbnail))
                    {
                        blog.Thumbnail = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                    }
                    else
                    {
                        blog.Thumbnail = await Ultils.GetUrlImage(blog.Thumbnail);
                    }
                });
                await Task.WhenAll(setThumbnail);
            };
            return ListOfBlog;
        }

    }
}
