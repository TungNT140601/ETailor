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
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/staff")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService staffService;
        private readonly IDashboardService dashboardService;
        private readonly IMapper mapper;
        private readonly string _wwwrootPath;
        public StaffController(IStaffService staffService, IMapper mapper, IWebHostEnvironment webHost, IDashboardService dashboardService)
        {
            this.staffService = staffService;
            this.mapper = mapper;

            _wwwrootPath = webHost.WebRootPath;
            this.dashboardService = dashboardService;
        }

        [HttpPost()]
        public async Task<IActionResult> AddStaff([FromForm] StaffCreateVM staff)
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
                        var staffCreate = mapper.Map<Staff>(staff);
                        return (await staffService.AddNewStaff(staffCreate, _wwwrootPath, staff.AvatarImage, staff.MasterySkill)) ? Ok("Tạo mới nhân viên thành công") : BadRequest("Tạo mới nhân viên thất bại");
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

        [HttpPut()]
        public async Task<IActionResult> UpdateStaffInfo(string? id, [FromForm] StaffUpdateVM staff)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null) //check login
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.ADMIN || role == RoleName.CUSTOMER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey)) // check token valid
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else if (role == RoleName.MANAGER) // check if manager login
                    {
                        if (id == null) // update manager's info
                        {
                            staff.Id = staffId;
                            return (await staffService.UpdateInfo(mapper.Map<Staff>(staff), _wwwrootPath, staff.AvatarImage, staff.MasterySkill, role)) ? Ok("Cập nhật thông tin thành công") : BadRequest("Cập nhật thông tin thất bại");
                        }
                        else // update staff info
                        {
                            if (staff.Id != id)
                            {
                                throw new UserException("Không tìm thấy nhân viên");
                            }
                            return (await staffService.UpdateInfo(mapper.Map<Staff>(staff), _wwwrootPath, staff.AvatarImage, staff.MasterySkill, role)) ? Ok("Cập nhật thông tin thành công") : BadRequest("Cập nhật thông tin thất bại");
                        }
                    }
                    else // staff login
                    {
                        staff.Id = staffId;
                        var staffUpdate = mapper.Map<Staff>(staff);
                        return (await staffService.UpdateInfo(mapper.Map<Staff>(staff), _wwwrootPath, staff.AvatarImage, staff.MasterySkill, role)) ? Ok("Cập nhật thông tin thành công") : BadRequest("Cập nhật thông tin thất bại");
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

        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePass([FromBody] StaffChangePassVM staff)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null) //check login
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.ADMIN || role == RoleName.CUSTOMER) // check role
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey)) // check token valid
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return await staffService.ChangePass(staffId, staff.OldPassword, staff.NewPassword) ? Ok("Thay đổi mật khẩu thành công") : BadRequest("Thay đổi mật khẩu thất bại");
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
        public IActionResult DeleteStaff(string id)
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
                        return staffService.RemoveStaff(id) ? Ok("Xóa nhân viên thành công") : BadRequest("Xóa nhân viên thất bại");
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

        [HttpGet("info")]
        public async Task<IActionResult> StaffInfo(string? id)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.CUSTOMER)
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
                        var staff = new Staff();
                        if (role == RoleName.STAFF)
                        {
                            staff = await staffService.GetStaff(staffId);
                        }
                        else
                        {
                            staff = await staffService.GetStaff(id == null ? staffId : id);
                        }

                        if (staff == null)
                        {
                            return NotFound();
                        }
                        else
                        {
                            var staffVM = mapper.Map<StaffListVM>(staff);
                            if (staff.Masteries != null && staff.Masteries.Any())
                            {
                                staffVM.MasterySkills = staff.Masteries.Select(x => x.CategoryId).ToList();
                            }
                            return Ok(staffVM);
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

        [HttpGet()]
        public async Task<IActionResult> GetAllStaff(string? search)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.CUSTOMER || role == RoleName.STAFF)
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
                        var staffs = staffService.GetAll(search);

                        if (staffs == null || !staffs.Any())
                        {
                            return Ok(new
                            {
                                TotalData = 0,
                                Data = new List<StaffListVM>()
                            });
                        }
                        else
                        {
                            var listTask = new List<Task>();

                            var staffVMs = new List<StaffListVM>();
                            int stt = 1;
                            foreach (var staff in staffs.OrderBy(x => x.Fullname))
                            {
                                listTask.Add(Task.Run(() =>
                                {
                                    var staffVM = mapper.Map<StaffListVM>(staff);
                                    staffVM.STT = stt;
                                    staffVM.Avatar = Ultils.GetUrlImage(staffVM.Avatar);
                                    staffVMs.Add(staffVM);
                                }));
                                stt++;
                            }

                            await Task.WhenAll(listTask);

                            return Ok(new
                            {
                                TotalData = staffs.Count(),
                                Data = staffVMs.OrderBy(x => x.STT)
                            });
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


        [HttpGet("current-tasks")]
        public async Task<IActionResult> GetStaffWithTotalTask()
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
                        return Ok(await dashboardService.GetStaffWithTotalTask());
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
