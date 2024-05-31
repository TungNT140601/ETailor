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
using System.Threading.Tasks;

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
                                var taskVM = mapper.Map<TaskDetailByStaffVM>(task);

                                if (taskVM != null && taskVM.ProductStages != null && taskVM.ProductStages.Any())
                                {
                                    return Ok(taskVM);
                                }
                                else if (taskVM != null && (taskVM.ProductStages == null || !taskVM.ProductStages.Any()))
                                {
                                    var tasks = new List<Task>();

                                    if (!string.IsNullOrWhiteSpace(task.SaveOrderComponents))
                                    {
                                        var productComponents = JsonConvert.DeserializeObject<List<ProductComponent>>(task.SaveOrderComponents);

                                        if (productComponents != null && productComponents.Any() && productComponents.Count > 0)
                                        {
                                            var componentIds = productComponents.Select(c => c.ComponentId).ToList();

                                            taskVM.ComponentTypeOrders = mapper.Map<List<ComponentTypeOrderVM>>((await productTemplateService.GetTemplateComponent(task.ProductTemplateId))?.ToList());

                                            if (taskVM.ComponentTypeOrders != null && taskVM.ComponentTypeOrders.Any() && taskVM.ComponentTypeOrders.Count > 0)
                                            {
                                                foreach (var component in taskVM.ComponentTypeOrders)
                                                {
                                                    tasks.Add(Task.Run(async () =>
                                                    {
                                                        var insideTasks = new List<Task>();

                                                        insideTasks.Add(Task.Run(() =>
                                                        {
                                                            component.Component_Id = $"component_{component.Id}";
                                                            component.Note_Id = $"productComponent_{component.Id}";
                                                        }));

                                                        insideTasks.Add(Task.Run(() =>
                                                        {
                                                            component.Components.RemoveAll(x => !componentIds.Contains(x.Id));
                                                        }));

                                                        insideTasks.Add(Task.Run(async () =>
                                                        {
                                                            var componentNote = productComponents.FirstOrDefault(x => component.Components.Select(c => c.Id).Contains(x.ComponentId));

                                                            component.Selected_Component_Id = componentNote?.ComponentId;

                                                            if (componentNote != null && (!string.IsNullOrEmpty(componentNote.Note) || !string.IsNullOrEmpty(componentNote.NoteImage)))
                                                            {

                                                                component.NoteObject = new ComponentNoteVM();

                                                                if (!string.IsNullOrEmpty(componentNote.Note))
                                                                {
                                                                    component.NoteObject.Note = componentNote.Note;
                                                                }

                                                                if (!string.IsNullOrEmpty(componentNote.NoteImage))
                                                                {
                                                                    var listImageDTO = JsonConvert.DeserializeObject<List<string>>(componentNote.NoteImage);
                                                                    if (listImageDTO != null && listImageDTO.Any())
                                                                    {
                                                                        var listImageUrl = new List<string>();
                                                                        var insideTasks1 = new List<Task>();
                                                                        foreach (var img in listImageDTO)
                                                                        {
                                                                            insideTasks1.Add(Task.Run(() =>
                                                                            {
                                                                                listImageUrl.Add(Ultils.GetUrlImage(img));
                                                                            }));
                                                                        }
                                                                        await Task.WhenAll(insideTasks1);
                                                                        component.NoteObject.NoteImage = JsonConvert.SerializeObject(listImageUrl);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                component.NoteObject = null;
                                                            }
                                                        }));

                                                        await Task.WhenAll(insideTasks);
                                                    }));
                                                }
                                            }
                                        }
                                    }

                                    await Task.WhenAll(tasks);

                                    return Ok(taskVM);
                                }
                                else
                                {
                                    return NotFound();
                                }
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
                                    if (category.ProductTemplates != null && category.ProductTemplates.Any(x => x.Products != null && x.Products.Any(x => x.Status > 0 && x.Status < 5)))
                                    {
                                        var templateTasks = new List<Task>();

                                        foreach (var template in category.ProductTemplates)
                                        {
                                            templateTasks.Add(Task.Run(() =>
                                            {
                                                if (template.Products != null && template.Products.Any(x => x.Status > 0 && x.Status < 5))
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
                                        category.ProductTemplates.RemoveAll(x => x.Products == null || !x.Products.Any(x => x.Status > 0 && x.Status < 5));

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
        [HttpPut("{taskId}/stage/{stageId}/material")]
        public async Task<IActionResult> SetMaterialForTask(string taskId, string stageId, [FromBody] List<ProductStageMaterialVM> materials)
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
                        var check = await taskService.SetMaterialForTask(taskId, stageId, mapper.Map<List<ProductStageMaterial>>(materials));
                        return check ? Ok("Thành công") : BadRequest("Thất bại");
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

        [HttpPut("auto/create-and-assign-task")]
        public async Task<IActionResult> RunAutoAssignTask()
        {
            try
            {
                await taskService.AutoCreateEmptyTaskProduct();

                await taskService.AutoAssignTaskForStaff();

                return Ok("Tự động phân nhiệm vụ thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
