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
using Newtonsoft.Json;
using System.Collections.Generic;

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
        private readonly IDashboardService dashboardService;
        private readonly IMapper mapper;
        private readonly string _wwwroot;

        public TaskController(ITaskService taskService, IProductService productService, IProductStageService productStageService,
            IStaffService staffService, ICustomerService customerService,
            IProfileBodyService profileBodyService, IBodySizeService bodySizeService, IBodyAttributeService bodyAttributeService,
            IMaterialService materialService, IProductTemplateService productTemplateService, ITemplateStageService templateStageService,
            IProductComponentService productComponentService, IComponentService componentService,
            IMapper mapper, IWebHostEnvironment webHost, IDashboardService dashboardService)
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
            this.dashboardService = dashboardService;
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

                            if (role == RoleName.MANAGER || task.StaffMakerId == staffId)
                            {
                                return Ok(mapper.Map<TaskDetailByStaffVM>(task));
                            }
                            else
                            {
                                return NotFound();
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
                        return check ? Ok("Kết thúc công việc thành công") : BadRequest("Kết thúc công việc thất bại");
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
                        var check = taskService.PendingTask(taskId, stageId, staffId);
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
        [HttpPut("task/{productId}/deadline")]
        public async Task<IActionResult> SetDeadlineForTask(string productId, string? deadlineTickString)
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
                        if (string.IsNullOrWhiteSpace(deadlineTickString))
                        {
                            return BadRequest("Thời hạn không hợp lệ: null");
                        }
                        long.TryParse(deadlineTickString, out long milliseconds);

                        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                        DateTime date = epoch.AddMilliseconds(milliseconds).AddHours(7);

                        if (date < DateTime.UtcNow.AddHours(7))
                        {
                            return BadRequest($"Thời hạn không hợp lệ :{date.ToString("yyyy/MM/dd - HH:mm:ss:ffff")}");
                        }
                        var check = await taskService.SetDeadlineForTask(productId, date);
                        return check ? Ok("Giao việc thành công") : BadRequest("Giao việc thất bại");
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

        [HttpPut("staff/{staffId}/assign/{productId}")]
        public async Task<IActionResult> AssignTaskToStaff(string productId, string staffId)
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
                        var check = await taskService.AssignTaskToStaff(productId, staffId);
                        return check ? Ok("Giao việc thành công") : BadRequest("Giao việc thất bại");
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
        [HttpPut("staff/{staffId}/unassign/{productId}")]
        public async Task<IActionResult> UnAssignTaskToStaff(string productId, string staffId)
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
                        var check = await taskService.UnAssignStaffTask(productId, staffId);
                        return check ? Ok("Hủy thành công") : BadRequest("Hủy thất bại");
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
        [HttpPut("swap-task/{taskId}")]
        public async Task<IActionResult> SwapTaskIndex(string taskId, string? staffId, int? index)
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
                        if (!string.IsNullOrWhiteSpace(staffId) && staffId == "unAssignedTasks")
                        {
                            staffId = null;
                        }
                        await taskService.SwapTaskIndex(taskId, staffId, index);
                        return Ok("Đổi thành công");
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
        [HttpGet("dashboard")]
        public async Task<IActionResult> TaskDashboard()
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
                        var categorieVMs = mapper.Map<IEnumerable<CategoryAllTaskVM>>((await taskService.GetTaskByCategories())?.OrderBy(x => x.Name));
                        if (categorieVMs != null && categorieVMs.Any())
                        {
                            var categoryTasks = new List<Task>();
                            foreach (var category in categorieVMs)
                            {
                                categoryTasks.Add(Task.Run(async () =>
                                {
                                    if (category.ProductTemplates != null && category.ProductTemplates.Any())
                                    {
                                        var templateTasks = new List<Task>();
                                        foreach (var template in category.ProductTemplates)
                                        {
                                            templateTasks.Add(Task.Run(() =>
                                            {
                                                if (template.Products != null && template.Products.Any())
                                                {
                                                    template.TotalTask = template.Products.Count;
                                                }
                                                else
                                                {
                                                    template.TotalTask = 0;
                                                }
                                            }));
                                        }
                                        await Task.WhenAll(templateTasks);

                                        category.ProductTemplates = category.ProductTemplates?.OrderByDescending(x => x.TotalTask).ToList();

                                        category.TotalTask = category.ProductTemplates.Sum(x => x.TotalTask);
                                    }
                                    else
                                    {
                                        category.TotalTask = 0;
                                    }
                                }));
                            }
                            await Task.WhenAll(categoryTasks);

                            categorieVMs = categorieVMs?.OrderByDescending(x => x.TotalTask).ToList();
                        }
                        return Ok(categorieVMs);
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
