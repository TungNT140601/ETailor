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
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly StorageClient _storage;
        private readonly string _wwwrootPath;
        public StaffController(IStaffService staffService, IConfiguration configuration, IMapper mapper, IWebHostEnvironment webHost)
        {
            this.staffService = staffService;
            this.configuration = configuration;
            this.mapper = mapper;
            // Load Firebase credentials
            var credential = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), AppValue.FIREBASE_KEY));

            // Initialize StorageClient with Firebase credentials
            _storage = StorageClient.Create(credential);

            _wwwrootPath = webHost.WebRootPath;
        }

        [HttpPost()]
        public async Task<IActionResult> AddStaff([FromBody] StaffCreateVM staff)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized();
                }
                else if (role != RoleName.MANAGER)
                {
                    return Forbid();
                }
                else
                {
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        var staffCreate = mapper.Map<Staff>(staff);
                        staffCreate.Avatar = await Ultils.UploadImage(_storage, _wwwrootPath, "StaffAvatar", staff.AvatarImage);
                        return staffService.AddNewStaff(staffCreate) ? Ok() : BadRequest();
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
        public async Task<IActionResult> UpdateStaffInfo(string? id, [FromBody] StaffUpdateVM staff)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized();
                }
                else if (role == RoleName.ADMIN || role == RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else if (role == RoleName.MANAGER)
                    {
                        var staffUpdate = mapper.Map<Staff>(staff);
                        staffUpdate.Avatar = await Ultils.UploadImage(_storage, _wwwrootPath, "StaffAvatar", staff.AvatarImage);
                        return staffService.UpdateInfo(staffUpdate) ? Ok() : BadRequest();
                    }
                    else
                    {
                        staff.Id = staffId;
                        var staffUpdate = mapper.Map<Staff>(staff);
                        staffUpdate.Avatar = await Ultils.UploadImage(_storage, _wwwrootPath, "StaffAvatar", staff.AvatarImage);
                        return staffService.UpdateInfo(staffUpdate) ? Ok() : BadRequest();
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
                if (role == null)
                {
                    return Unauthorized();
                }
                else if (role == RoleName.ADMIN || role == RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        if (role == RoleName.MANAGER && id == null)
                        {
                            throw new UserException("Id nhân viên không được để trống");
                        }
                        else
                        {
                            return staffService.ChangePass(id, staff.OldPassword, staff.NewPassword, role) ? Ok() : BadRequest();
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
                    return Unauthorized();
                }
                else if (role != RoleName.MANAGER)
                {
                    return Forbid();
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized();
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
                    return Unauthorized();
                }
                else if (role == RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized();
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

        [HttpGet()]
        public IActionResult GetAllStaff(string? search)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized();
                }
                else if (role == RoleName.CUSTOMER || role == RoleName.STAFF)
                {
                    return Forbid();
                }
                else
                {
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else
                    {
                        var staffs = staffService.GetAll(search).ToList();
                        return Ok(new
                        {
                            TotalData = staffs.Count(),
                            Data = mapper.Map<List<StaffListVM>>(staffs)
                        });
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
