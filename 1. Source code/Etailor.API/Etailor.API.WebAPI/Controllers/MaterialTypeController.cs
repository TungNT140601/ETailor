using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/material-type")]
    [ApiController]
    public class MaterialTypeController : ControllerBase
    {
        private readonly IMaterialTypeService materialTypeService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;

        public MaterialTypeController(IMaterialTypeService materialTypeService, IStaffService staffService, IMapper mapper)
        {
            this.materialTypeService = materialTypeService;
            this.mapper = mapper;
            this.staffService = staffService;
        }
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            try
            {
                var materialType = materialTypeService.GetMaterialType(id);
                if (materialType == null)
                {
                    return NotFound("không tìm thấy loại nguyên liệu");
                }
                else
                {
                    return Ok(mapper.Map<MaterialTypeVM>(materialType));
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
                var materialTypes = materialTypeService.GetMaterialTypes(search);
                return Ok(mapper.Map<IEnumerable<MaterialTypeAllVM>>(materialTypes));
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
        public IActionResult CreateMaterialType([FromBody] MaterialTypeVM materialType)
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
                     return Forbid("Không có quyền truy cập");
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
                        if (string.IsNullOrWhiteSpace(materialType.Name))
                        {
                            throw new UserException("Nhập tên loại nguyên liệu.");
                        }
                        else
                        {
                            return materialTypeService.CreateMaterialType(mapper.Map<MaterialType>(materialType)) ? Ok("Tạo mới loại nguyên liệu thành công") : BadRequest("Tạo mới loại nguyên liệu thất bại");
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
        public IActionResult UpdateMaterialType(string? id, [FromBody] MaterialTypeVM materialType)
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
                     return Forbid("Không có quyền truy cập");
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
                        if (string.IsNullOrWhiteSpace(materialType.Name))
                        {
                            throw new UserException("Nhập tên loại nguyên liệu.");
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(materialType.Name))
                            {
                                throw new UserException("Nhập tên loại nguyên liệu.");
                            }
                            else if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(materialType.Id) || id != materialType.Id)
                            {
                                return NotFound("Không tìm thấy loại nguyên liệu");
                            }
                            else
                            {
                                return materialTypeService.UpdateMaterialType(mapper.Map<MaterialType>(materialType)) ? Ok("Cập nhật loại nguyên liệu thành công") : BadRequest("Cập nhật loại nguyên liệu thất bại");
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
        public IActionResult DeleteMaterialType(string? id)
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
                     return Forbid("Không có quyền truy cập");
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
                            return NotFound("Không tìm thấy loại nguyên liệu");
                        }
                        else
                        {
                            return materialTypeService.DeleteMaterialType(id) ? Ok("Xóa loại nguyên liệu thành công") : BadRequest("Xóa loại nguyên liệu thất bại");
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
