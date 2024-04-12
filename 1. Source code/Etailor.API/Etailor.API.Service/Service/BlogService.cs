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

            var tasks = new List<Task>();

            var dupplicate = blogRepository.GetAll(x => !string.IsNullOrWhiteSpace(blog.Title) && x.Title == blog.Title && x.IsActive == true);
            if (dupplicate != null && dupplicate.Any())
            {
                dupplicate = dupplicate.ToList();
            }
            else
            {
                dupplicate = null;
            }

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(blog.Title) || blog.Title == "undefined")
                {
                    throw new UserException("Vui lòng nhập tên tiêu đề");
                }
                else if (dupplicate != null && dupplicate.Any())
                {
                    throw new UserException("Tên bài viết đã được sử dụng");
                }
                else
                {
                    blog.UrlPath = Ultils.ConvertToEnglishAlphabet(blog.Title);
                    if (blog.UrlPath.Contains("/"))
                    {
                        blog.UrlPath = blog.UrlPath.Replace("/", "-");
                    }
                }
            }));

            tasks.Add(Task.Run(async () =>
            {
                if (thumbnail != null)
                {
                    blog.Thumbnail = await Ultils.UploadImage(wwwroot, "Blog", thumbnail, null);
                }
                else
                {
                    throw new UserException("Vui lòng chọn 1 ảnh cho bài viết");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(blog.Content) || blog.Content == "undefined")
                {
                    throw new UserException("Vui lòng điền nội dung bài viết");
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                blog.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                blog.CreatedTime = DateTime.UtcNow.AddHours(7);
                blog.InactiveTime = null;
                blog.IsActive = true;
            }));

            await Task.WhenAll(tasks);

            return blogRepository.Create(blog);
        }

        public async Task<bool> UpdateBlog(Blog blog, string wwwroot, IFormFile? thumbnail)
        {
            var existBlog = blogRepository.Get(blog.Id);
            if (existBlog != null && existBlog.IsActive == true)
            {
                var dupplicate = blogRepository.GetAll(x => x.Id != existBlog.Id && !string.IsNullOrWhiteSpace(blog.Title) && x.Title == blog.Title && x.IsActive == true);
                if (dupplicate != null && dupplicate.Any())
                {
                    dupplicate = dupplicate.ToList();
                }
                else
                {
                    dupplicate = null;
                }

                existBlog.StaffId = blog.StaffId;

                var tasks = new List<Task>();

                tasks.Add(Task.Run(async () =>
                {
                    if (thumbnail != null)
                    {
                        existBlog.Thumbnail = await Ultils.UploadImage(wwwroot, "Blog", thumbnail, null);
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(blog.Title) || blog.Title == "undefined")
                    {
                        throw new UserException("Vui lòng nhập tên tiêu đề");
                    }
                    else if (dupplicate != null && dupplicate.Any())
                    {
                        throw new UserException("Tên bài viết đã được sử dụng");
                    }
                    else
                    {
                        existBlog.Title = blog.Title;
                        existBlog.UrlPath = Ultils.ConvertToEnglishAlphabet(blog.Title);
                        if (existBlog.UrlPath.Contains("/"))
                        {
                            existBlog.UrlPath = existBlog.UrlPath.Replace("/", "-");
                        }
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (string.IsNullOrWhiteSpace(blog.Content) || blog.Content == "undefined")
                    {
                        throw new UserException("Vui lòng điền nội dung bài viết");
                    }
                    else
                    {
                        existBlog.Content = blog.Content;
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    if (!string.IsNullOrWhiteSpace(blog.Hastag))
                    {
                        if (!blog.Hastag.StartsWith("#"))
                        {
                            blog.Hastag = "#" + blog.Hastag;
                        }

                        if (blog.Hastag.Contains(" "))
                        {
                            blog.Hastag = blog.Hastag.Replace(" ", "");
                        }

                        existBlog.Hastag = blog.Hastag;
                    }
                    else
                    {
                        throw new UserException("Vui lòng không để trống Hashtag");
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    existBlog.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                    existBlog.InactiveTime = null;
                    existBlog.IsActive = true;
                }));

                await Task.WhenAll(tasks);

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
            if (existBlog != null && existBlog.IsActive == true)
            {
                existBlog.LastestUpdatedTime = DateTime.UtcNow.AddHours(7);
                existBlog.InactiveTime = DateTime.UtcNow.AddHours(7);
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
            if (blog != null && blog.IsActive == true)
            {
                var setThumbnail = Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(blog.Thumbnail))
                    {
                        blog.Thumbnail = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                    }
                    else
                    {
                        blog.Thumbnail = Ultils.GetUrlImage(blog.Thumbnail);
                    }
                });
                await Task.WhenAll(setThumbnail);

                return blog;
            }
            return null;
        }
        public async Task<Blog> GetBlogUrl(string urlPath)
        {
            var blog = blogRepository.FirstOrDefault(x => x.UrlPath == urlPath && x.IsActive == true);
            if (blog != null && blog.IsActive == true)
            {
                var setThumbnail = Task.Run(async () =>
                {
                    if (string.IsNullOrEmpty(blog.Thumbnail))
                    {
                        blog.Thumbnail = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                    }
                    else
                    {
                        blog.Thumbnail = Ultils.GetUrlImage(blog.Thumbnail);
                    }
                });
                await Task.WhenAll(setThumbnail);

                return blog;
            }
            return null;
        }

        public async Task<IEnumerable<Blog>> GetBlogs(string? search)
        {
            var ListOfBlog = blogRepository.GetAll(x => (search == null || (search != null && x.Title.Trim().ToLower().Contains(search.Trim().ToLower()))) && x.IsActive == true);
            if (ListOfBlog != null && ListOfBlog.Any())
            {
                ListOfBlog = ListOfBlog.OrderByDescending(x => x.CreatedTime).OrderBy(x => x.Title).ToList();
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
                            blog.Thumbnail = Ultils.GetUrlImage(blog.Thumbnail);
                        }
                    });
                    await Task.WhenAll(setThumbnail);
                };
            }
            return ListOfBlog;
        }

        public async Task<IEnumerable<Blog>> GetRelativeBlog(string? hastag)
        {
            var listOfBlogs = blogRepository.GetAll(x => hastag != null && x.Hastag == hastag && x.IsActive == true);
            if (listOfBlogs != null && listOfBlogs.Any())
            {
                var random = new Random();
                var shuffledBlogs = listOfBlogs.OrderBy(x => random.Next()).Take(3).ToList();

                foreach (Blog blog in shuffledBlogs)
                {
                    var setThumbnail = Task.Run(() =>
                    {
                        if (string.IsNullOrEmpty(blog.Thumbnail))
                        {
                            blog.Thumbnail = "https://firebasestorage.googleapis.com/v0/b/etailor-21a50.appspot.com/o/Uploads%2FThumbnail%2Fstill-life-spring-wardrobe-switch.jpg?alt=media&token=7dc9a197-1b76-4525-8dc7-caa2238d8327";
                        }
                        else
                        {
                            blog.Thumbnail = Ultils.GetUrlImage(blog.Thumbnail);
                        }
                    });
                    await Task.WhenAll(setThumbnail);
                };

                return shuffledBlogs;
            }
            return new List<Blog>();
        }
    }
}
