using AutoMapper;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Ultity.CommonValue;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/task")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly IProductStageService productStageService;
        private readonly IStaffService staffService;
        private readonly ICustomerService customerService;
        private readonly ITaskService taskService;
        private readonly IProfileBodyService profileBodyService;
        private readonly IBodySizeService bodySizeService;
        private readonly IBodyAttributeService bodyAttributeService;
        private readonly IMaterialService materialService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IMapper mapper;
        private readonly string _wwwroot;

        public TaskController(ITaskService taskService, IProductService productService, IProductStageService productStageService,
            IStaffService staffService, ICustomerService customerService,
            IProfileBodyService profileBodyService, IBodySizeService bodySizeService,IBodyAttributeService bodyAttributeService, 
            IMaterialService materialService, IProductTemplateService productTemplateService,
            IMapper mapper, IWebHostEnvironment webHost)
        {
            this.taskService = taskService;
            this.productService = productService;
            this.staffService = staffService;
            this.customerService = customerService;
            this.productStageService = productStageService;
            this.profileBodyService = profileBodyService;
            this.bodySizeService = bodySizeService;
            this.bodyAttributeService = bodyAttributeService;
            this.materialService = materialService;
            this.productTemplateService = productTemplateService;
            this.mapper = mapper;
            this._wwwroot = webHost.WebRootPath;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(string id)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (!(role == RoleName.STAFF))
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
                        if (id == null)
                        {
                            return NotFound("Id sản phẩm không tồn tại");
                        }
                        else
                        {
                            var t = await taskService.GetTask(id);
                            if (t!= null && t.StaffMakerId == staffId)
                            {
                                var task = mapper.Map<TaskDetailByStaffVM>(t);

                                task.ProductTemplateName = (await productTemplateService.GetById(task.ProductTemplateId)).Name;

                                task.ProfileBodyName = profileBodyService.GetProfileBody(task.ReferenceProfileBodyId).Name;

                                var bodyAttributeList = bodyAttributeService.GetBodyAttributesByProfileBodyId(task.ReferenceProfileBodyId)
                                                                            .Select(x => new { x.Value, x.BodySize, x.BodySizeId }).ToList();

                                BodySize bodySize;
                                //var bodySizeList = bodySizeService.GetBodySize("");

                                ////var bodySizeList = await bodySizeService.GetBodySize(bodyAttribute.BodySizeId);
                                task.ProfileBodyValue = new List<ProfileBodyDetailVM>();

                                foreach (var bodyAttribute in bodyAttributeList)
                                {
                                    bodySize = await bodySizeService.GetBodySize(bodyAttribute.BodySizeId);

                                    ProfileBodyDetailVM profileBodyDetail = new ProfileBodyDetailVM();
                                    profileBodyDetail.Id = bodyAttribute.BodySizeId;
                                    profileBodyDetail.Name = bodySize.Name;
                                    profileBodyDetail.Value = (decimal)bodyAttribute.Value;
                                    task.ProfileBodyValue.Add(profileBodyDetail);
                                }

                                var material = materialService.GetMaterial(task.FabricMaterialId);
                                task.MaterialName = material.Name;
                                task.MaterialQuantity = material.Quantity;
                                var setImage = Task.Run(async () =>
                                {
                                    task.MaterialImage = await Ultils.GetUrlImage(material.Image);
                                });
                                await Task.WhenAll(setImage);

                                return task != null ? Ok(task) : NotFound(id);
                            }
                            else
                            {
                                throw new UserException("ID công việc này không phải của bạn. Vui lòng nhập lại ");
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

        [HttpGet("/staff/product-stages/{taskId}")]
        public async Task<IActionResult> GetProductStageNeedForTask(string? taskId)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (!(role == RoleName.STAFF))
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
                        var productStagesNeedForTasks = mapper.Map<IEnumerable<ProductStagesNeedForTask>>(await taskService.GetProductStagesOfEachTask(taskId));
                        return Ok(productStagesNeedForTasks);
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

        //For manager to get all
        [HttpGet("/manager/get-all")]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (!(role == RoleName.MANAGER))
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
                        var tasks = mapper.Map<IEnumerable<TaskListVM>>(await taskService.GetTasks());
                        return Ok(tasks);
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

        [HttpGet("/staff/get-all")]
        public async Task<IActionResult> GetTasksByStaffId()
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (!(role == RoleName.STAFF))
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
                        var tasks = mapper.Map<IEnumerable<TaskListByStaffVM>>(await taskService.GetTasksByStaffId(staffId));
                        return Ok(tasks);
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
