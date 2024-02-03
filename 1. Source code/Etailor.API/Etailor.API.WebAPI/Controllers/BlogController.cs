using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/blog")]
    [ApiController]
    public class BlogController : Controller
    {
        private readonly IBlogService blogService;
        private readonly IMapper mapper;
        private readonly ICategoryService categoryService;

        public BlogController(IBlogService blogService, IMapper mapper, ICategoryService categoryService)
        {
            this.blogService = blogService;
            this.mapper = mapper;
            this.categoryService = categoryService;

        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            try
            {
                var blog = blogService.GetBlog(id);
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
        public IActionResult GetAll(string? search)
        {
            try
            {
                var blogs = blogService.GetBlogs(search);
                return Ok(mapper.Map<IEnumerable<BlogVM>>(blogs));
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
        public IActionResult CreateBlog([FromBody] BlogVM blogVM)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.MANAGER)
                //{
                //    return Forbid("Không có quyền truy cập");
                //}
                //else
                //{
                //    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                if (string.IsNullOrWhiteSpace(blogVM.Title))
                {
                    throw new UserException("Nhập tên tiêu đề");
                }
                else
                {
                    return blogService.CreateBlog(mapper.Map<Blog>(blogVM)) ? Ok("Tạo mới bài blog thành công") : BadRequest("Tạo mới bài blog thất bại");
                }
                //    }
                //}
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
        public IActionResult UpdateBodySize(string? id, [FromBody] BlogVM blogVM)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.MANAGER)
                //{
                //    return Forbid("Không có quyền truy cập");
                //}
                //else
                //{
                //    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                //        if (string.IsNullOrWhiteSpace(discount.Name) || string.IsNullOrEmpty(discount.Code))
                //        {
                //            throw new UserException("Nhập tên phiếu giảm giá");
                //        }
                //        else
                //        {
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(blogVM.Id) || id != blogVM.Id)
                {
                    return NotFound("Không tìm thấy bài blog");
                }
                else
                {
                    return blogService.UpdateBlog(mapper.Map<Blog>(blogVM)) ? Ok("Cập nhật bài blog thành công") : BadRequest("Cập nhật bài blog thất bại");
                }
                //        }
                //    }
                //}
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
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.MANAGER)
                //{
                //    return Forbid("Không có quyền truy cập");
                //}
                //else
                //{
                //    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound("Không tìm thấy bài blog");
                }
                else
                {
                    return blogService.DeleteBlog(id) ? Ok("Xóa bài blog thành công") : BadRequest("Xóa bài blog thất bại");
                }
                //    }
                //}
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
