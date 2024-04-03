using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        private readonly IBodyAttributeService bodyAttributeService;
        private readonly IBodySizeService bodySizeService;

        public ProfileBodyController(IProfileBodyService profileBodyService, ICustomerService customerService, IStaffService staffService, IMapper mapper,
            IBodyAttributeService bodyAttributeService, IBodySizeService bodySizeService)
        {
            this.profileBodyService = profileBodyService;
            this.customerService = customerService;
            this.staffService = staffService;
            this.mapper = mapper;
            this.bodyAttributeService = bodyAttributeService;
            this.bodySizeService = bodySizeService;
        }

        [HttpPost()]
        public async Task<IActionResult> AddProfileBodyByStaff([FromBody] CreateProfileBodyVM createProfileBodyByStaffVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.ADMIN)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;

                    if ((!staffService.CheckSecrectKey(id, secrectKey) && (role == RoleName.STAFF || role == RoleName.MANAGER)) || (!customerService.CheckSecerctKey(id, secrectKey) && role == RoleName.CUSTOMER))
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
                            List<(string id, decimal? value)> list = new List<(string id, decimal? value)>();
                            var listBodyAttribute = createProfileBodyByStaffVM.valueBodyAttribute;
                            foreach (var item in listBodyAttribute)
                            {
                                list.Add((item.Id, item.Value));
                            }

                            if (role == RoleName.STAFF || role == RoleName.MANAGER)
                            {
                                return await profileBodyService.CreateProfileBodyByStaff(createProfileBodyByStaffVM.CustomerId, id, createProfileBodyByStaffVM.Name, list) ? Ok("Thêm Profile Body thành công") : BadRequest("Thêm Profile Body thất bại");
                            }
                            else if (role == RoleName.CUSTOMER)
                            {
                                return await profileBodyService.CreateProfileBodyByCustomer(id, createProfileBodyByStaffVM.Name, list) ? Ok("Thêm Profile Body thành công") : BadRequest("Thêm Profile Body thất bại");
                            }
                            else
                            {
                                throw new UserException("Something went wrong!!!");
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

        [HttpPut("customer/{profileBodyId}")]
        public async Task<IActionResult> UpdateProfileBod(string profileBodyId, [FromBody] UpdateProfileBodyByStaffVM updateProfileBodyByStaffVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.ADMIN)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var accountId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;

                    if ((role == RoleName.STAFF || role == RoleName.MANAGER) && (!staffService.CheckSecrectKey(accountId, secrectKey)) || (role == RoleName.CUSTOMER && !customerService.CheckSecerctKey(accountId, secrectKey)))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(updateProfileBodyByStaffVM.Name))
                        {
                            throw new UserException("Nhập tên hồ sơ số đo cơ thể");
                        }
                        else
                        {
                            var listAttribute = new List<BodyAttribute>();
                            var listBodyAttribute = updateProfileBodyByStaffVM.valueBodyAttribute;

                            foreach (var item in listBodyAttribute)
                            {
                                listAttribute.Add(new BodyAttribute
                                {
                                    BodySizeId = item.Id,
                                    Value = item.Value
                                }); 
                            }

                            if (role == RoleName.STAFF || role == RoleName.MANAGER)
                            {
                                return (await profileBodyService.UpdateProfileBodyByStaff(updateProfileBodyByStaffVM.CustomerId, accountId, updateProfileBodyByStaffVM.Name, profileBodyId, listAttribute, mapper.Map<ProfileBody>(updateProfileBodyByStaffVM)))
                                    ? Ok("Cập nhật Profile Body thành công") : BadRequest("Cập nhật Profile Body thất bại");
                            }
                            else if (role == RoleName.CUSTOMER)
                            {
                                return (await profileBodyService.UpdateProfileBodyByCustomer(accountId, updateProfileBodyByStaffVM.Name, profileBodyId, listAttribute, mapper.Map<ProfileBody>(updateProfileBodyByStaffVM)))
                                    ? Ok("Cập nhật Profile Body thành công") : BadRequest("Cập nhật Profile Body thất bại");
                            }
                            else
                            {
                                throw new UserException("Something went wrong!!!");
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfileBody(string id)
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
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var staffid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!customerService.CheckSecerctKey(staffid, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        if (id == null)
                        {
                            return NotFound("Id sản phẩm không tồn tại");
                        }
                        return (await profileBodyService.DeleteProfileBody(id)) ? Ok("Xóa Profile Body thành công") : BadRequest("Xóa Profile Body thất bại");
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileBody(string id)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.ADMIN)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    var customerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if ((role == RoleName.CUSTOMER && !customerService.CheckSecerctKey(customerId, secrectKey)) || (role != RoleName.CUSTOMER && !staffService.CheckSecrectKey(customerId, secrectKey)))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        if (id == null)
                        {
                            return NotFound("Id Profile Body không tồn tại");
                        }
                        else
                        {
                            var pB = await profileBodyService.GetProfileBody(id);

                            if (pB != null && ((role == RoleName.CUSTOMER && pB.CustomerId == customerId) || role != RoleName.CUSTOMER))
                            {
                                var profileBody = mapper.Map<GetDetailProfileBodyVM>(pB);

                                return pB != null ? Ok(profileBody) : NotFound(id);
                            }
                            else
                            {
                                return NotFound("Không tìm thấy hồ sơ số đo");
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

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetProfileBodysByStaffId(string? staffId) //Lay ProfileBody theo staffId, -> ai dam nhiem
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

        [HttpGet()]
        public async Task<IActionResult> GetProfileBodysOfCustomer()
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (!(role == RoleName.CUSTOMER))
                {
                    return Unauthorized("Không có quyền truy cập");
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
                        var profileBodyList = mapper.Map<IEnumerable<GetAllProfileBodyOfCustomerVM>>(profileBodyService.GetProfileBodysByCustomerId(id));
                        foreach (var profileBody in profileBodyList)
                        {
                            if (staffService.GetStaff(profileBody.StaffId) != null)
                            {
                                profileBody.StaffName = (await staffService.GetStaff(profileBody.StaffId))?.Fullname;
                            }
                            else
                            {
                                profileBody.StaffName = null;
                            }
                            if (customerService.FindById(id) != null)
                            {
                                profileBody.CustomerName = customerService.FindById(id).Fullname;
                            }
                            else
                            {
                                profileBody.CustomerName = null;
                            }

                        }
                        return Ok(profileBodyList);
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

        [HttpGet("staff/customer/{customerId}")]
        public async Task<IActionResult> GetProfileBodysOfCustomerByStaff(string customerId)
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
                    var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!staffService.CheckSecrectKey(id, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return Ok(mapper.Map<IEnumerable<GetAllProfileBodyOfCustomerVM>>(profileBodyService.GetProfileBodysByCustomerId(customerId)));
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
