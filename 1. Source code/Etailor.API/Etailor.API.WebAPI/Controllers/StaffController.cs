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
        private readonly IMapper mapper;
        private readonly string _wwwrootPath;
        public StaffController(IStaffService staffService, IMapper mapper, IWebHostEnvironment webHost)
        {
            this.staffService = staffService;
            this.mapper = mapper;

            _wwwrootPath = webHost.WebRootPath;
        }

        [HttpPost()]
        public async Task<IActionResult> AddStaff([FromForm] StaffCreateVM staff)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                ////else if (role != RoleName.MANAGER)
                //else if (role == RoleName.STAFF || role == RoleName.CUSTOMER)
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
                        var staffCreate = mapper.Map<Staff>(staff);
                        return (await staffService.AddNewStaff(staffCreate, _wwwrootPath, staff.AvatarImage, staff.MasterySkill)) ? Ok("Tạo mới nhân viên thành công") : BadRequest("Tạo mới nhân viên thất bại");
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
                //else if (role == RoleName.ADMIN || role == RoleName.CUSTOMER)
                else if (role == RoleName.CUSTOMER) //check role
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
                    //else if (role == RoleName.MANAGER) // check if manager login
                    else if (role == RoleName.MANAGER || role == RoleName.ADMIN)
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
        public IActionResult ChangePass(string? id, [FromBody] StaffChangePassVM staff)
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
                    if (!staffService.CheckSecrectKey(id, secrectKey)) // check token valid
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        if (role == RoleName.MANAGER && id == null) // check if manager change staff pass
                        {
                            throw new UserException("Id nhân viên không được để trống");
                        }
                        else
                        {
                            return staffService.ChangePass(id, staff.OldPassword, staff.NewPassword, role) ? Ok("Thay đổi mật khẩu thành công") : BadRequest("Thay đổi mật khẩu thất bại");
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
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return Ok();
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
        public IActionResult StaffInfo(string? id)
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
                        if (role == RoleName.STAFF)
                        {
                            return Ok(mapper.Map<StaffListVM>(staffService.GetStaff(staffId)));
                        }
                        else
                        {
                            var staff = staffService.GetStaff(id == null ? staffId : id);
                            if (staff == null)
                            {
                                return NotFound();
                            }
                            else
                            {
                                return Ok(mapper.Map<StaffListVM>(staff));
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

        [HttpGet("info")]
        public async Task<IActionResult> GetStaffInfo(string? id)
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
                        return Ok(mapper.Map<StaffListVM>(staffService.GetStaffInfo(staffId)));
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
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role == RoleName.CUSTOMER || role == RoleName.STAFF)
                //{
                //    return Unauthorized("Không có quyền truy cập");
                //}
                //else
                //{
                //    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                        var staffs = staffService.GetAll(search).ToList();

                        var listTask = new List<Task>();

                        var staffVMs = new List<StaffListVM>();
                        int stt = 1;
                        foreach (var staff in staffs)
                        {
                            var staffVM = mapper.Map<StaffListVM>(staff);
                            staffVM.STT = stt;
                            stt++;
                            listTask.Add(GetUrlImageAsync(staffVM));
                            staffVMs.Add(staffVM);
                        }

                        await Task.WhenAll(listTask);

                        return Ok(new
                        {
                            TotalData = staffs.Count(),
                            Data = staffVMs
                        });
                    //}
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

        private async Task GetUrlImageAsync(StaffListVM staff)
        {
            if (!string.IsNullOrEmpty(staff.Avatar))
            {
                staff.Avatar = await Ultils.GetUrlImage(staff.Avatar);
            }
        }
    }
}
