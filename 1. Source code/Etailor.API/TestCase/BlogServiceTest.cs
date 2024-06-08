using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.Repository;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCase
{
    public class BlogServiceTest
    {
        IBlogService blogService;
        ETailor_DBContext dBContext = new ETailor_DBContext();

        IBlogRepository blogRepository;


        [SetUp]
        public void Setup()
        {
            blogRepository = new BlogRepository(dBContext);

            blogService = new BlogService(blogRepository);
        }

        //GetBlogList By Name (title of blog)
        //IsActive == true is nested in code func GetBlogs of BlogService
        [Test]
        public async Task BlogList_WithNullNameData()
        {
            var result = await blogService.GetBlogs(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Blog>>(result, "The result should be a list of Discount.");
        }

        [Test]
        public async Task BlogList_WithEmptyNameData()
        {
            var result = await blogService.GetBlogs("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Blog>>(result, "The result should be a list of Blog.");
        }

        [Test]
        public async Task BlogList_WithNameData()
        {
            var result = await blogService.GetBlogs("thời trang");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Blog>>(result, "The result should be a list of Blog.");
        }

        //GetBlogList By Name (title of blog)
        //IsActive == true is nested in code func GetBlogs of BlogService
        [Test]
        public async Task BlogList_WithNullHashtagData()
        {
            var result = await blogService.GetRelativeBlog(null);

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Blog>>(result, "The result should be a list of Discount.");
        }

        [Test]
        public async Task BlogList_WithEmptyHashtagData()
        {
            var result = await blogService.GetRelativeBlog("");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Blog>>(result, "The result should be a list of Blog.");
        }

        [Test]
        public async Task BlogList_WithHashtagData()
        {
            var result = await blogService.GetRelativeBlog("#CuocSongHienDai");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<IEnumerable<Blog>>(result, "The result should be a list of Blog.");
        }

        //GetBlog By Id 
        //IsActive == true is nested in code func GetBlog of BlogService
        [Test]
        public async Task BlogDetail_WithNullIdData()
        {
            var result = await blogService.GetBlog(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task BlogDetail_WithEmptyIdData()
        {
            var result = await blogService.GetBlog("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task BlogDetail_WithIdData()
        {
            var result = await blogService.GetBlog("8e7df3f6-754d-4d66-bc5a-90fcd8");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Blog>(result, "The result should be as Blog object.");
        }

        //GetBlog By UrlPath 
        //IsActive == true is nested in code func GetBlog of BlogService
        [Test]
        public async Task BlogDetail_WithNullUrlPathData()
        {
            var result = await blogService.GetBlogUrl(null);

            Assert.IsNull(result, "The result should be null");
        }

        [Test]
        public async Task BlogDetail_WithEmptyUrlPathData()
        {
            var result = await blogService.GetBlogUrl("");

            Assert.AreEqual(null, result, "The result should be null");
        }

        [Test]
        public async Task BlogDetail_WithUrlPathData()
        {
            var result = await blogService.GetBlogUrl("tam-quan-trong-cua-thoi-trang-trong-cuoc-song-hien-đai");

            Assert.IsNotNull(result, "The result should not be null.");

            Assert.IsInstanceOf<Blog>(result, "The result should be as Blog object.");
        }
    }
}
