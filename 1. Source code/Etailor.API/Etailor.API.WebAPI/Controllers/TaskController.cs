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
using Etailor.API.Repository.Repository;

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
        private readonly ITemplateStageService templateStageService;
        private readonly IProductComponentService productComponentService;
        private readonly IComponentService componentService;
        private readonly IMapper mapper;
        private readonly string _wwwroot;

        public TaskController(ITaskService taskService, IProductService productService, IProductStageService productStageService,
            IStaffService staffService, ICustomerService customerService,
            IProfileBodyService profileBodyService, IBodySizeService bodySizeService, IBodyAttributeService bodyAttributeService,
            IMaterialService materialService, IProductTemplateService productTemplateService, ITemplateStageService templateStageService,
            IProductComponentService productComponentService, IComponentService componentService,
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
            this.templateStageService = templateStageService;
            this.productComponentService = productComponentService;
            this.componentService = componentService;
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
                else if (role != RoleName.STAFF && role != RoleName.MANAGER)
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
                            var task = await taskService.GetTask(id);

                            if (task == null || task.StaffMakerId != staffId)
                            {
                                return NotFound();
                            }
                            else
                            {
                                return Ok(mapper.Map<TaskDetailByStaffVM>(task));
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

        [HttpGet("staff/product-stages/{taskId}")]
        public async Task<IActionResult> GetProductStageNeedForTask(string? taskId)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.STAFF && role != RoleName.MANAGER)
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
                        var task = await taskService.GetProductStagesOfEachTask(taskId);

                        var productStagesNeedForTasks = mapper.Map<IEnumerable<ProductStagesNeedForTask>>(task);

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
        [HttpGet("manager/get-all")]
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

        [HttpGet("staff/get-all")]
        public async Task<IActionResult> GetTasksByStaffId()
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.STAFF && role != RoleName.MANAGER)
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

        [HttpPut("staff/{taskId}/start/{stageId}")]
        public async Task<IActionResult> StartStage(string taskId, string stageId)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.STAFF)
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
                        var check = await taskService.StartTask(taskId, stageId, staffId);
                        return check ? Ok() : BadRequest("Bắt đầu công việc thất bại");
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

        [HttpPut("staff/{taskId}/finish/{stageId}")]
        public async Task<IActionResult> FinishStage(string taskId, string stageId, [FromForm] List<IFormFile>? images)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.STAFF)
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
                        var check = await taskService.FinishTask(_wwwroot, taskId, stageId, staffId, images);
                        return check ? Ok() : BadRequest("Kết thúc công việc thất bại");
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

        [HttpPut("staff/{taskId}/pending/{stageId}")]
        public async Task<IActionResult> PendingStage(string taskId, string stageId)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.STAFF)
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
                        var check = true;
                        return check ? Ok() : BadRequest("Tạm dừng công việc thất bại");
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
