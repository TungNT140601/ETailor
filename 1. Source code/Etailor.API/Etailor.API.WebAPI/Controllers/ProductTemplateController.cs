using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/template-management")]
    [ApiController]
    public class ProductTemplateController : ControllerBase
    {
        private readonly IProductTemplateService productTemplateService;
        private readonly ICategoryService categoryService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;
        private readonly string _wwwroot;

        public ProductTemplateController(IProductTemplateService productTemplateService, ICategoryService categoryService, IMapper mapper, IStaffService staffService, IWebHostEnvironment webHost)
        {
            this.productTemplateService = productTemplateService;
            this.categoryService = categoryService;
            this.mapper = mapper;
            this.staffService = staffService;
            _wwwroot = webHost.WebRootPath;
        }

        [HttpGet("get-all-template")]
        public async Task<IActionResult> GetAllTemplate()
        {
            try
            {
                var categories = mapper.Map<IEnumerable<CategoryAllTemplateVM>>(categoryService.GetCategorys(null));
                var returnData = new List<CategoryAllTemplateVM>();
                if (categories != null && categories.Any())
                {
                    foreach (var category in categories)
                    {
                        category.ProductTemplates = new List<ProductTemplateALLVM>();

                        var templates = await productTemplateService.GetByCategory(category.Id);
                        if(templates != null)
                        {
                            foreach (var template in templates)
                            {
                                var templateVM = mapper.Map<ProductTemplateALLVM>(template);

                                await Task.WhenAll(Task.Run(() =>
                                {
                                    if (!string.IsNullOrWhiteSpace(template.Image))
                                    {
                                        templateVM.Images = JsonConvert.DeserializeObject<List<string>>(template.Image);
                                    }
                                }), Task.Run(() =>
                                {
                                    if (!string.IsNullOrWhiteSpace(template.CollectionImage))
                                    {
                                        templateVM.CollectionImages = JsonConvert.DeserializeObject<List<string>>(template.CollectionImage);
                                    }
                                }));

                                category.ProductTemplates.Add(templateVM);
                            }
                        }
                        if (category.ProductTemplates != null && category.ProductTemplates.Any())
                        {
                            returnData.Add(category);
                        }
                    }
                }
                return Ok(returnData);
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

        [HttpGet("category/{id}")]
        public async Task<IActionResult> GetByCategoryId(string id)
        {
            try
            {
                if (categoryService.GetCategory(id) != null)
                {
                    var category = mapper.Map<CategoryAllTemplateVM>(categoryService.GetCategory(id));

                    var templates = await productTemplateService.GetByCategory(category.Id);

                    foreach (var template in templates)
                    {
                        var templateVM = mapper.Map<ProductTemplateALLVM>(template);
                        await Task.WhenAll(Task.Run(() =>
                        {
                            if (!string.IsNullOrWhiteSpace(template.Image))
                            {
                                templateVM.Images = JsonConvert.DeserializeObject<List<string>>(template.Image);
                            }
                        }), Task.Run(() =>
                        {
                            if (!string.IsNullOrWhiteSpace(template.CollectionImage))
                            {
                                templateVM.CollectionImages = JsonConvert.DeserializeObject<List<string>>(template.CollectionImage);
                            }
                        }));

                        category.ProductTemplates.Add(templateVM);
                    }

                    return Ok(category);
                }
                else
                {
                    return NotFound();
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

        [HttpGet("get-template/{urlPath}")]
        public async Task<IActionResult> GetByUrlPath(string urlPath)
        {
            try
            {
                var template = await productTemplateService.GetByUrlPath(urlPath);
                if (template == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(new
                    {
                        Template = mapper.Map<ProductTemplateALLVM>(template),
                        Component = mapper.Map<IEnumerable<ComponentTypeOrderVM>>(productTemplateService.GetTemplateComponent(template.Id))
                    });
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

        [HttpPost("create-template")]
        public async Task<IActionResult> CreateTemplate([FromForm] ProductTemplateCreateVM templateCreateVM)
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
                //    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(id, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                return Ok(await productTemplateService.AddTemplate(mapper.Map<ProductTemplate>(templateCreateVM), _wwwroot, templateCreateVM.ThumbnailImageFile, templateCreateVM.ImageFiles, templateCreateVM.CollectionImageFiles));
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

        [HttpPost("{id}/body-size-template")]
        public async Task<IActionResult> CreateBodySize(string id, [FromBody] List<string>? bodySizes)
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
                //    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(id, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                return Ok(bodySizes);
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

        [HttpPost("{id}/create-template-stage")]
        public async Task<IActionResult> CreateStage(string id, [FromBody] TemplateStageCreateVM createVM)
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
                //    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(id, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                return Ok(createVM);
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

        [HttpPut("update-template/{id}")]
        public async Task<IActionResult> UpdateTemplate(string id, [FromForm] ProductTemplateCreateVM templateCreateVM)
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
                //    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(id, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                return Ok(await productTemplateService.UpdateDraftTemplate(mapper.Map<ProductTemplate>(templateCreateVM), _wwwroot, templateCreateVM.ThumbnailImageFile, templateCreateVM.ImageFiles, templateCreateVM.CollectionImageFiles));
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

        [HttpDelete("delete-template/{id}")]
        public async Task<IActionResult> DeleteTemplate(string id)
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
                //    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(id, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                return Ok(productTemplateService.DeleteTemplate(id));
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
