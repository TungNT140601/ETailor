using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/template-stage")]
    [ApiController]
    public class TemplateStageController : ControllerBase
    {
        private readonly ITemplateStageService templateStageService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;
        private readonly string _wwwroot;
        public TemplateStageController(ITemplateStageService templateStageService, IMapper mapper, IStaffService staffService, IWebHostEnvironment webHost, IProductTemplateService productTemplateService)
        {
            this.templateStageService = templateStageService;
            this.mapper = mapper;
            this.staffService = staffService;
            _wwwroot = webHost.WebRootPath;
            this.productTemplateService = productTemplateService;
        }

        [HttpGet("template/{templateId}")]
        public IActionResult GetAll(string templateId)
        {
            try
            {
                var stages = templateStageService.GetAll(templateId, null);
                return Ok(mapper.Map<List<TemplateStageAllVM>>(stages));
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

        [HttpPost("template/{templateId}")]
        public async Task<IActionResult> CreateTemplateStages(string templateId, [FromBody] List<TemplateStageCreateVM> stageCreateVMs)
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
                int i = 1;
                var stages = new List<TemplateStage>();
                foreach (var stageCreateVM in stageCreateVMs)
                {
                    var stage = mapper.Map<TemplateStage>(stageCreateVM);
                    if (string.IsNullOrWhiteSpace(stage.Name))
                    {
                        throw new UserException("Tên giai đoạn không được để trống");
                    }
                    else
                    {
                        stage.ProductTemplateId = templateId;
                        stage.StageNum = i;
                        i++;
                        stage.ComponentStages = new List<ComponentStage>();
                        foreach (var id in stageCreateVM.ComponentTypeIds)
                        {
                            stage.ComponentStages.Add(new ComponentStage
                            {
                                ComponentTypeId = id
                            });
                        }
                        stages.Add(stage);
                    }
                }
                if (await templateStageService.CreateTemplateStages(templateId, stages))
                {
                    return productTemplateService.CreateSaveActiveTemplate(templateId) ? Ok() : BadRequest();
                }
                else
                {
                    return BadRequest();
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

        [HttpPut("template/{templateId}")]
        public async Task<IActionResult> UpdateTemplateStages(string templateId, [FromBody] List<TemplateStageCreateVM> stageCreateVMs)
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
                int i = 1;
                var stages = new List<TemplateStage>();
                foreach (var stageCreateVM in stageCreateVMs)
                {
                    var stage = mapper.Map<TemplateStage>(stageCreateVM);
                    if (string.IsNullOrWhiteSpace(stage.Name))
                    {
                        throw new UserException("Tên giai đoạn không được để trống");
                    }
                    else
                    {
                        stage.ProductTemplateId = templateId;
                        stage.StageNum = i;
                        i++;
                        stage.ComponentStages = new List<ComponentStage>();
                        foreach (var id in stageCreateVM.ComponentTypeIds)
                        {
                            stage.ComponentStages.Add(new ComponentStage
                            {
                                ComponentTypeId = id
                            });
                        }
                        stages.Add(stage);
                    }
                }
                return (await templateStageService.UpdateTemplateStages(templateId, stages, _wwwroot)) ? Ok() : BadRequest();
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
