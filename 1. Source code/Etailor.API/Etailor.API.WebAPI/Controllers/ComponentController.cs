using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Security.Claims;
using Component = Etailor.API.Repository.EntityModels.Component;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentController : ControllerBase
    {
        private readonly IComponentService componentService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;
        private readonly string _wwwroot;

        public ComponentController(IComponentService componentService,
            IStaffService staffService, IMapper mapper,
            IWebHostEnvironment webHost, IProductTemplateService productTemplateService)
        {
            this.componentService = componentService;
            this.staffService = staffService;
            this.mapper = mapper;
            _wwwroot = webHost.WebRootPath;
            this.productTemplateService = productTemplateService;
        }

        [HttpPost("template/{templateId}/{componentTypeId}")]
        public async Task<IActionResult> CreateComponent(string templateId, string componentTypeId, [FromForm] ComponentVM componentVM)
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
                        var component = mapper.Map<Component>(componentVM);
                        component.ProductTemplateId = templateId;
                        component.ComponentTypeId = componentTypeId;
                        return Ok(await componentService.AddComponent(component, componentVM.ImageFile, _wwwroot));
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

        [HttpPut("template/{templateId}/{componentTypeId}/{id}")]
        public async Task<IActionResult> UpdateComponent(string templateId, string componentTypeId, string id, [FromForm] ComponentVM componentVM)
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
                        var component = mapper.Map<Component>(componentVM);
                        component.Id = id;
                        component.ProductTemplateId = templateId;
                        component.ComponentTypeId = componentTypeId;
                        return Ok(await componentService.UpdateComponent(component, componentVM.ImageFile, _wwwroot));
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

        [HttpDelete("template/{templateId}/{componentTypeId}/{id}")]
        public async Task<IActionResult> DeleteComponent(string templateId, string componentTypeId, string id)
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
                        return Ok(componentService.DeleteComponent(id));
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

        [HttpPut("/api/template/{id}/check/component-types")]
        public async Task<IActionResult> CheckDefaultComponent(string id)
        {
            try
            {
                return await componentService.CheckDefaultComponent(id) ? Ok() : BadRequest();
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

        [HttpGet("export/template/{templateId}")]
        public async Task<IActionResult> ExportFile(string templateId)
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
                        var filePath = productTemplateService.ExportFile(templateId);

                        if (!System.IO.File.Exists(filePath))
                        {
                            return NotFound("File not found");
                        }
                        else
                        {
                            var fileName = Path.GetFileName(filePath);

                            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                            System.IO.File.Delete(filePath);

                            return File(fileBytes, "application/octet-stream", fileName);
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
        [HttpPost("import/template/{templateId}")]
        public async Task<IActionResult> ExportFile(string templateId, IFormFile? file)
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
                        if (file == null || file.Length == 0)
                        {
                            return BadRequest("Không có file");
                        }
                        else
                        {
                            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ".xls",
                                ".xlsx"
                            };

                            var excelMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                            {
                                "application/vnd.ms-excel",
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                            };

                            string fileExtension = Path.GetExtension(file.FileName).ToLower();
                            string mimeType = file.ContentType.ToLower();

                            if (allowedExtensions.Contains(fileExtension) && excelMimeTypes.Contains(mimeType))
                            {
                                return await componentService.ImportFileAddComponents(templateId, file, _wwwroot) ? Ok("Thêm dữ liệu kiểu bộ phận thành công") : BadRequest("Thêm dữ liệu kiểu bộ phận thất bại");
                            }
                            else
                            {
                                return BadRequest($"File không đúng định dạng: {fileExtension}");
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
    }
}
