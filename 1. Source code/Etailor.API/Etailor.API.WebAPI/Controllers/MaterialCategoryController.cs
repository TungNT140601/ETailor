using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Repository;
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
        private readonly IMaterialTypeService materialTypeService;

        public MaterialCategoryController(IMaterialCategoryService materialCategoryService, IStaffService staffService, IMapper mapper,
            IMaterialTypeService materialTypeService)
        {
            this.materialCategoryService = materialCategoryService;
            this.mapper = mapper;
            this.staffService = staffService;
            this.materialTypeService = materialTypeService;
        }
        [HttpGet("{id}")]
        public IActionResult Get(string? id)
        {
            try
            {
                var materialCategory = mapper.Map<MaterialCategoryVM>(materialCategoryService.GetMaterialCategory(id));
                
                var materialType = materialTypeService.GetMaterialType(materialCategory.MaterialTypeId).Name;

                materialCategory.MaterialTypeName = materialType;

                return materialCategory != null ? Ok(materialCategory) : NotFound("không tìm thấy loại danh mục");
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
                var materialCategoryList = mapper.Map<IEnumerable<MaterialCategoryVM>>(materialCategoryService.GetMaterialCategorys(search));

                var materialTypeList = materialTypeService.GetMaterialTypes("").Select(x => new {x.Id, x.Name});

                foreach (var materialCategory in materialCategoryList)
                {
                    materialCategory.MaterialTypeName = materialTypeList.FirstOrDefault(x => x.Id == materialCategory.MaterialTypeId).Name;
                }

                return Ok(materialCategoryList);
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
