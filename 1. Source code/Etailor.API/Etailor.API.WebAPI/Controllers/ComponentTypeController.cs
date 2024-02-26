using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/component-type-management")]
    [ApiController]
    public class ComponentTypeController : ControllerBase
    {
        private readonly IStaffService staffService;
        private readonly IComponentTypeService componentTypeService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IMapper mapper;

        public ComponentTypeController(IStaffService staffService, IMapper mapper, IComponentTypeService componentTypeService, IProductTemplateService productTemplateService)
        {
            this.staffService = staffService;
            this.mapper = mapper;
            this.componentTypeService = componentTypeService;
            this.productTemplateService = productTemplateService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComponentType([FromBody] ComponentTypeFormVM componentType)
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
                //var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //if (!staffService.CheckSecrectKey(id, secrectKey))
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else
                //{
                return (await componentTypeService.AddComponentType(mapper.Map<ComponentType>(componentType))) ? Ok("Tạo mới loại danh mục thành công") : BadRequest("Tạo mới loại danh mục thất bại");
                //}
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
        public async Task<IActionResult> UpdateComponentType(string? id, [FromBody] ComponentTypeFormVM componentType)
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
                //    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                if (id == null || id != componentType.Id)
                {
                    return NotFound("Không tìm thấy loại danh mục");
                }
                else
                {
                    return (await componentTypeService.UpdateComponentType(mapper.Map<ComponentType>(componentType))) ? Ok("Cập nhật loại danh mục thành công") : BadRequest("Cập nhật loại danh mục thất bại");
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComponentType(string? id)
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
                //    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                if (id == null)
                {
                    return NotFound("Không tìm thấy loại danh mục");
                }
                else
                {
                    return componentTypeService.DeleteComponentType(id) ? Ok("Xóa loại danh mục thành công") : BadRequest("Xóa loại danh mục thất bại");
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetComponentType(string? id)
        {
            try
            {
                //if (id == null)
                //{
                //    return NotFound("Không tìm thấy loại danh mục");
                //}
                //else
                //{
                var componentType = componentTypeService.GetComponentType(id);
                if (componentType == null)
                {
                    return NotFound("Không tìm thấy loại danh mục");
                }
                else
                {
                    return Ok(mapper.Map<ComponentTypeVM>(componentType));
                }
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

        [HttpGet]
        public async Task<IActionResult> GetComponentTypes(string? search)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<ComponentTypeVM>>(componentTypeService.GetComponentTypes(search)));
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

        [HttpGet("/api/category/{id}/component-types")]
        public async Task<IActionResult> GetComponentTypesByCategory(string id)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<ComponentTypeVM>>(componentTypeService.GetComponentTypesByCategory(id)));
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

        [HttpGet("/api/template/{id}/component-types")]
        public async Task<IActionResult> GetComponentTypesByTemplate(string id)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<ComponentTypeOrderVM>>(productTemplateService.GetTemplateComponent(id).OrderBy(x => x.Name)));
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
