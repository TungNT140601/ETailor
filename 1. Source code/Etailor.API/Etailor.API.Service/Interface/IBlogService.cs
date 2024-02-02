using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IBlogService
    {
        bool CreateBlog(Blog blog);

        bool UpdateBlog(Blog blog);

        bool DeleteBlog(string id);

        Blog GetBlog(string id);

        IEnumerable<Blog> GetBlogs(string? search);
    }
}
