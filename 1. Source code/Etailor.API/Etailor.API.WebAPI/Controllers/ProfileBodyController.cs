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
                            List<(string id, decimal value)> list = new List<(string id, decimal value)>();
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
                    return Unauthorized("Không có quyền truy cập");
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
                //    return Unauthorized("Không có quyền truy cập");
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
                    var pB = profileBodyService.GetProfileBody(id);

                    var profileBody = mapper.Map<GetDetailProfileBodyVM>(pB);

                    DetailProfileBody detailProfileBody = new DetailProfileBody();

                    var bodyAttributeList = bodyAttributeService.GetBodyAttributesByProfileBodyId(id).Select(x => new { x.Value, x.BodySize, x.BodySizeId }).ToList();

                    BodySize bodySize;
                    //var bodySizeList = bodySizeService.GetBodySize("");

                    ////var bodySizeList = await bodySizeService.GetBodySize(bodyAttribute.BodySizeId);
                    profileBody.valueBodyAttribute = new List<DetailProfileBody>();

                    foreach (var bodyAttribute in bodyAttributeList)
                    {
                        bodySize = await bodySizeService.GetBodySize(bodyAttribute.BodySizeId);
                        detailProfileBody.Id = bodyAttribute.BodySizeId;
                        detailProfileBody.Name = bodySize.Name;
                        detailProfileBody.Value = (decimal) bodyAttribute.Value;
                        detailProfileBody.Image = bodySize.Image;
                        profileBody.valueBodyAttribute.Add(detailProfileBody);
                    }

                    return pB != null ? Ok(profileBody) : NotFound(id);
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

        //[HttpGet("staff/customer{customerId}")]
        //public async Task<IActionResult> GetProfileBodysByCustomerId(string customerId)
        //{
        //    try
        //    {
        //        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        //        if (role == null)
        //        {
        //            return Unauthorized("Chưa đăng nhập");
        //        }
        //        else if (role == RoleName.CUSTOMER || role == RoleName.STAFF)
        //        {
        //            return Unauthorized("Không có quyền truy cập");
        //        }
        //        else
        //        {
        //            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //            var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
        //            if (!staffService.CheckSecrectKey(id, secrectKey))
        //            {
        //                return Unauthorized("Chưa đăng nhập");
        //            }
        //            else
        //            {
        //                return Ok(mapper.Map<IEnumerable<ProfileBodyVM>>(profileBodyService.GetProfileBodysByCustomerId(customerId)));
        //            }
        //        }

        //    }
        //    catch (UserException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (SystemsException ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

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
                        return Ok(mapper.Map<IEnumerable<ProfileBodyVM>>(profileBodyService.GetProfileBodysByCustomerId(id)));
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
                        return Ok(mapper.Map<IEnumerable<ProfileBodyVM>>(profileBodyService.GetProfileBodysByCustomerId(customerId)));
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


        //[HttpPost("/add-profile-body-by-customer")]
        //public async Task<IActionResult> AddProfileBodyByCustomer([FromBody] CreateProfileBodyByCustomerVM createProfileBodyByCustomerVM)
        //{
        //    try
        //    {
        //        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        //        if (role == null)
        //        {
        //            return Unauthorized("Chưa đăng nhập");
        //        }
        //        else if (role != RoleName.CUSTOMER)
        //        {
        //            return Unauthorized("Không có quyền truy cập");
        //        }
        //        else
        //        {
        //            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //            var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
        //            if (!customerService.CheckSecerctKey(id, secrectKey))
        //            {
        //                return Unauthorized("Chưa đăng nhập");
        //            }
        //            else
        //            {
        //                if (string.IsNullOrWhiteSpace(createProfileBodyByCustomerVM.Name))
        //                {
        //                    throw new UserException("Nhập tên hồ sơ số đo cơ thể");
        //                }
        //                else
        //                {
        //                    List<(string id, decimal value)> list = new List<(string id, decimal value)>();
        //                    var listBodyAttribute = createProfileBodyByCustomerVM.valueBodyAttribute;
        //                    foreach (var item in listBodyAttribute)
        //                    {
        //                        list.Add((item.Id, item.Value));
        //                    }
        //                    return await profileBodyService.CreateProfileBodyByCustomer(id, createProfileBodyByCustomerVM.Name, list) ? Ok("Thêm Profile Body thành công") : BadRequest("Thêm Profile Body thất bại");
        //                }
        //            }
        //        }
        //    }
        //    catch (UserException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (SystemsException ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}
    }
}
