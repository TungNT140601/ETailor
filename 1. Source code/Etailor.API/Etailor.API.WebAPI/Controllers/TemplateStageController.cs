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
        public async Task<IActionResult> CreateTemplateStages(string templateId, [FromBody] List<TemplateStageCreateVM>? stageCreateVMs)
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
                        var stages = new List<TemplateStage>();
                        if (stageCreateVMs != null && stageCreateVMs.Count > 0)
                        {
                            for (int i = 0; i < stageCreateVMs.Count; i++)
                            {
                                var stage = mapper.Map<TemplateStage>(stageCreateVMs[i]);
                                if (string.IsNullOrWhiteSpace(stage.Name))
                                {
                                    throw new UserException("Tên giai đoạn không được để trống");
                                }
                                else
                                {
                                    stage.ProductTemplateId = templateId;
                                    stage.StageNum = i + 1;
                                    stage.ComponentStages = new List<ComponentStage>();
                                    if (stageCreateVMs[i].ComponentTypeIds != null && stageCreateVMs[i].ComponentTypeIds.Count > 0)
                                    {
                                        for (int j = 0; j < stageCreateVMs[i].ComponentTypeIds.Count; j++)
                                        {
                                            stage.ComponentStages.Add(new ComponentStage
                                            {
                                                ComponentTypeId = stageCreateVMs[i].ComponentTypeIds[j]
                                            });
                                        }
                                    }
                                    stages.Add(stage);
                                }
                            }

                            if (await templateStageService.CreateTemplateStages(templateId, stages))
                            {
                                return productTemplateService.CreateSaveActiveTemplate(templateId) ? Ok("Tạo bản mẫu thành công") : BadRequest("Tạo bản mẫu thất bại");
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }
                        else
                        {
                            throw new UserException("Không có giai đoạn nào được tạo");
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
                var stages = new List<TemplateStage>();
                if (stageCreateVMs != null && stageCreateVMs.Count > 0)
                {
                    var tasks = new List<Task>();
                    for (int i = 0; i < stageCreateVMs.Count; i++)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            var stage = mapper.Map<TemplateStage>(stageCreateVMs[i]);
                            if (string.IsNullOrWhiteSpace(stage.Name))
                            {
                                throw new UserException("Tên giai đoạn không được để trống");
                            }
                            else
                            {
                                stage.ProductTemplateId = templateId;
                                stage.StageNum = i + 1;
                                stage.ComponentStages = new List<ComponentStage>();
                                if (stageCreateVMs[i].ComponentTypeIds != null && stageCreateVMs[i].ComponentTypeIds.Count > 0)
                                {
                                    var insideTasks = new List<Task>();
                                    for (int j = 0; j < stageCreateVMs[i].ComponentTypeIds.Count; j++)
                                    {
                                        insideTasks.Add(Task.Run(async () =>
                                        {
                                            stage.ComponentStages.Add(new ComponentStage
                                            {
                                                ComponentTypeId = stageCreateVMs[i].ComponentTypeIds[j]
                                            });
                                        }));
                                    }
                                    await Task.WhenAll(insideTasks);
                                }
                                stages.Add(stage);
                            }
                        }));
                    }
                    await Task.WhenAll(tasks);

                    return (await templateStageService.UpdateTemplateStages(templateId, stages, _wwwroot)) ? Ok("Cập nhật bản mẫu thành công") : BadRequest("Cập nhật bản mẫu thất bại");
                }
                else
                {
                    throw new UserException("Không có giai đoạn nào được tạo");
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
