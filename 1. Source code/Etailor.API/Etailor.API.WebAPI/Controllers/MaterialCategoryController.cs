using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/material-category")]
    [ApiController]
    public class MaterialCategoryController : ControllerBase
    {
        private readonly IMaterialCategoryService materialCategoryService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;

        public MaterialCategoryController(IMaterialCategoryService materialCategoryService, IStaffService staffService, IMapper mapper)
        {
            this.materialCategoryService = materialCategoryService;
            this.mapper = mapper;
            this.staffService = staffService;
        }
        [HttpGet("{id}")]
        public IActionResult Get(string? id)
        {
            try
            {
                var materialCategory = materialCategoryService.GetMaterialCategory(id);
                return materialCategory != null ? Ok(mapper.Map<MaterialCategoryVM>(materialCategory)) : NotFound("không tìm thấy loại danh mục");
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
                var materialCategory = materialCategoryService.GetMaterialCategorys(search);
                return Ok(mapper.Map<IEnumerable<MaterialCategoryVM>>(materialCategory));
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
        public async Task<IActionResult> CreateMaterialCategory([FromBody] CreateMaterialCategoryVM createMaterialCategoryVM)
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
                //    return Unauthorized("Không có quyền truy cập");
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
                return await materialCategoryService.CreateMaterialCategory(mapper.Map<MaterialCategory>(createMaterialCategoryVM)) ? Ok("Tạo mới loại nguyên liệu thành công") : BadRequest("Tạo mới loại nguyên liệu thất bại");
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
        public async Task<IActionResult> UpdateMaterialCategory(string? id, [FromBody] UpdateMaterialCategoryVM updateMaterialCategoryVM)
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
                //    return Unauthorized("Không có quyền truy cập");
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
                return await materialCategoryService.UpdateMaterialCategory(mapper.Map<MaterialCategory>(updateMaterialCategoryVM)) ? Ok("Cập nhật loại nguyên liệu thành công") : BadRequest("Cập nhật loại nguyên liệu thất bại");
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
        public IActionResult DeleteMaterialCategory(string? id)
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
                //    return Unauthorized("Không có quyền truy cập");
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
                return materialCategoryService.DeleteMaterialCategory(id) ? Ok("Xóa loại nguyên liệu thành công") : BadRequest("Xóa loại nguyên liệu thất bại");
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
