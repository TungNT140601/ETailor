using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/material")]
    [ApiController]
    public class MaterialController : Controller
    {
        private readonly IMaterialService materialService;
        private readonly IOrderMaterialService orderMaterialService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;
        private readonly string _wwwroot;

        public MaterialController(IMaterialService materialService, IMapper mapper, IWebHostEnvironment webHost, IStaffService staffService, IOrderMaterialService orderMaterialService)
        {
            this.materialService = materialService;
            this.mapper = mapper;
            this._wwwroot = webHost.WebRootPath;
            this.staffService = staffService;
            this.orderMaterialService = orderMaterialService;
        }

        [HttpPost]
        public async Task<IActionResult> AddMaterial([FromForm] MaterialFormVM materialVM)
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
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return (await materialService.AddMaterial(mapper.Map<Material>(materialVM), materialVM.ImageFile, _wwwroot)) ? Ok("Thêm nguyên liệu thành công") : BadRequest("Thêm nguyên liệu thất bại");
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
        public async Task<IActionResult> UpdateMaterial(string id, [FromForm] MaterialFormVM materialVM)
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
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return (await materialService.UpdateMaterial(mapper.Map<Material>(materialVM), materialVM.ImageFile, _wwwroot)) ? Ok("Cập nhật nguyên liệu thành công") : BadRequest("Cập nhật nguyên liệu thất bại");
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
        public async Task<IActionResult> DeleteMaterial(string id)
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
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return (materialService.DeleteMaterial(id)) ? Ok("Xóa nguyên liẹu thành công") : BadRequest("Xóa nguyên liệu thất bại");
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaterial(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id nguyên liệu không tồn tại");
                }
                else
                {
                    var material = mapper.Map<MaterialVM>(materialService.GetMaterial(id));
                    material.Image = Ultils.GetUrlImage(material.Image);
                    return material != null ? Ok(material) : NotFound(id);
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

        [HttpGet("material-category/{categoryId}")]
        public async Task<IActionResult> GetMaterialsByMaterialCategory(string categoryId)
        {
            try
            {
                var materials = mapper.Map<IEnumerable<MaterialVM>>(materialService.GetMaterialsByMaterialCategory(categoryId));
                if (materials != null && materials.Any())
                {
                    foreach (var material in materials)
                    {
                        material.Image = Ultils.GetUrlImage(material.Image);
                    }
                }
                return Ok(materials);
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

        [HttpGet("material-type/{typeId}")]
        public async Task<IActionResult> GetMaterialsByMaterialType(string typeId)
        {
            try
            {
                var materials = mapper.Map<IEnumerable<MaterialVM>>(materialService.GetMaterialsByMaterialType(typeId));
                if (materials != null && materials.Any())
                {
                    foreach (var material in materials)
                    {
                        material.Image = Ultils.GetUrlImage(material.Image);
                    }
                }
                return Ok(materials);
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
        public async Task<IActionResult> GetMaterials(string? search)
        {
            try
            {
                var materials = materialService.GetMaterials(search).ToList();
                if (materials != null && materials.Any())
                {
                    foreach (var material in materials)
                    {
                        material.Image = Ultils.GetUrlImage(material.Image);
                    }
                }

                return Ok(mapper.Map<IEnumerable<MaterialVM>>(materials));
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

        [HttpGet("order/{orderId}/fabric")]
        public async Task<IActionResult> GetAllFabricMaterials(string orderId, string? search)
        {
            try
            {
                var materials = mapper.Map<IEnumerable<MaterialVM>>((await materialService.GetFabricMaterials(search))?.ToList());
                var orderMaterials = orderMaterialService.GetMaterialIdOfOrder(orderId);
                return Ok(new
                {
                    OrderMaterials = materials.Where(m => orderMaterials.Contains(m.Id)),
                    Materials = materials.Where(m => !orderMaterials.Contains(m.Id))
                });
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
