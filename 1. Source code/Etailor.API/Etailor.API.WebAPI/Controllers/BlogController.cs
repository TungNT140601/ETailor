using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Etailor.API.Ultity.CommonValue;
using Microsoft.AspNetCore;
using Etailor.API.Ultity;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/blog")]
    [ApiController]
    public class BlogController : Controller
    {
        private readonly IBlogService blogService;
        private readonly IStaffService staffService;
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;
        private readonly string _wwwrootPath;

        public BlogController(IBlogService blogService, IMapper mapper, ICategoryService categoryService, IStaffService staffService, IWebHostEnvironment webHost)
        {
            this.blogService = blogService;
            this.mapper = mapper;
            this.categoryService = categoryService;
            this.staffService = staffService;
            _wwwrootPath = webHost.WebRootPath;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var blog = await blogService.GetBlog(id);
                if (blog == null)
                {
                    return NotFound("không tìm thấy bài blog này");
                }
                else
                {
                    return Ok(mapper.Map<BlogVM>(blog));
                }
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? search)
        {
            try
            {
                var blogs = await blogService.GetBlogs(search);
                return Ok(mapper.Map<IEnumerable<ListOfBlogVM>>(blogs));
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromForm] CreateBlogVM blogVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var blog = mapper.Map<Blog>(blogVM);
                        if (string.IsNullOrWhiteSpace(blogVM.Title))
                        {
                            throw new UserException("Nhập tên tiêu đề");
                        }
                        else
                        {
                            blog.StaffId = staffId;
                            //blog.Thumbnail = "";
                            return (await blogService.CreateBlog(blog, _wwwrootPath, blogVM.Thumbnail)) ? Ok("Tạo mới bài blog thành công") : BadRequest("Tạo mới bài blog thất bại");
                        }
                    }
                }
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(string? id, [FromForm] UpdateBlogVM blogVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var blog = mapper.Map<Blog>(blogVM);
                        if (string.IsNullOrWhiteSpace(blog.Title))
                        {
                            throw new UserException("Nhập tiêu đề bài blog");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(blogVM.Id) || id != blogVM.Id)
                            {
                                return NotFound("Không tìm thấy bài blog");
                            }
                            else
                            {
                                blog.StaffId = staffId;
                                return (await blogService.UpdateBlog(blog, _wwwrootPath, blogVM.Thumbnail)) ? Ok("Cập nhật bài blog thành công") : BadRequest("Cập nhật bài blog thất bại");
                            }
                        }
                    }
                }
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBlog(string? id)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            return NotFound("Không tìm thấy bài blog");
                        }
                        else
                        {
                            return blogService.DeleteBlog(id) ? Ok("Xóa bài blog thành công") : BadRequest("Xóa bài blog thất bại");
                        }
                    }
                }
            }
            catch (UserException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (SystemsException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
