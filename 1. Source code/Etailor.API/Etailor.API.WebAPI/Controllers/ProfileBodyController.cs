using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/profile-body")]
    [ApiController]
    public class ProfileBodyController : ControllerBase
    {
        private readonly IProfileBodyService profileBodyService;
        private readonly ICustomerService customerService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;

        public ProfileBodyController(IProfileBodyService profileBodyService, ICustomerService customerService, IStaffService staffService, IMapper mapper)
        {
            this.profileBodyService = profileBodyService;
            this.customerService = customerService;
            this.staffService = staffService;
            this.mapper = mapper;
        }

        [HttpPost("/add-profile-body-by-staff")]
        
        public async Task<IActionResult> AddProfileBodyByStaff([FromBody] CreateProfileBodyByStaffVM createProfileBodyByStaffVM, string customerId)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (!(role == RoleName.STAFF || role == RoleName.MANAGER))
                {
                    return Forbid("Không có quyền truy cập");
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

                        if (string.IsNullOrWhiteSpace(createProfileBodyByStaffVM.Name))
                        {
                            throw new UserException("Nhập tên hồ sơ số đo cơ thể");
                        }
                        else
                        {
                            List<(string id, decimal value)> list = new List<(string id, decimal value)>();
                            var listBodyAttribute = createProfileBodyByStaffVM.valueBodyAttribute;
                            foreach (var item in listBodyAttribute)
                            {
                                list.Add((item.Id, item.Value));
                            }
                            return await profileBodyService.CreateProfileBody(customerId, id, createProfileBodyByStaffVM.Name, list) ? Ok("Thêm Profile Body thành công") : BadRequest("Thêm Profile Body thất bại");
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

        [HttpPost("/add-profile-body-by-customer")]
        public async Task<IActionResult> AddProfileBodyByCustomer([FromBody] CreateProfileBodyByCustomerVM profileBodyVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.CUSTOMER)
                {
                    return Forbid("Không có quyền truy cập");
                }
                else
                {
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!customerService.CheckSecerctKey(id, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var profileBody = mapper.Map<ProfileBody>(profileBodyVM);
                        if (string.IsNullOrWhiteSpace(profileBodyVM.Name))
                        {
                            throw new UserException("Nhập tên hồ sơ số đo cơ thể");
                        }
                        else
                        {
                            profileBody.CustomerId = id;
                            profileBody.StaffId = null;
                            profileBody.IsLocked = false;
                            return await profileBodyService.AddProfileBody(profileBody) ? Ok("Thêm Profile Body thành công") : BadRequest("Thêm Profile Body thất bại");
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfileBody(string id, [FromBody] UpdateProfileBodyVM profileBodyVM)
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
                    return Forbid("Không có quyền truy cập");
                }
                else
                {
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        if (id == null || id != profileBodyVM.Id)
                        {
                            return NotFound("Id danh mục không tồn tại");
                        }
                        return (await profileBodyService.UpdateProfileBody(mapper.Map<ProfileBody>(profileBodyVM))) ? Ok("Cập nhật Profile Body thành công") : BadRequest("Cập nhật Profile Body thất bại");
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
        public async Task<IActionResult> DeleteProfileBody(string id)
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
                //    return Forbid("Không có quyền truy cập");
                //}
                //else
                //{
                //var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //if (!staffService.CheckSecrectKey(staffid, secrectKey))
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else
                //{
                if (id == null)
                {
                    return NotFound("Id sản phẩm không tồn tại");
                }
                return (await profileBodyService.DeleteProfileBody(id)) ? Ok("Xóa Profile Body thành công") : BadRequest("Xóa Profile Body thất bại");
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileBody(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound("Id Profile Body không tồn tại");
                }
                else
                {
                    var profileBody = profileBodyService.GetProfileBody(id);
                    return profileBody != null ? Ok(mapper.Map<ProfileBodyVM>(profileBody)) : NotFound(id);
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

        [HttpGet("/get-profile-body-by-customer-id")]
        public async Task<IActionResult> GetProfileBodysByCustomerId(string? customerId)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<ProfileBodyVM>>(profileBodyService.GetProfileBodysByCustomerId(customerId)));
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

        [HttpGet("/get-profile-body-by-staff-id")]
        public async Task<IActionResult> GetProfileBodysByStaffId(string? staffId)
        {
            try
            {
                return Ok(mapper.Map<IEnumerable<ProfileBodyVM>>(profileBodyService.GetProfileBodysByStaffId(staffId)));
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
