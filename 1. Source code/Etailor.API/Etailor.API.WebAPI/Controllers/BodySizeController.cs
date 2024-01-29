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
    [Route("api/body-size")]
    [ApiController]
    public class BodySizeController : ControllerBase
    {
        private readonly IBodySizeService bodySizeService;
        private readonly IMapper mapper;

        public BodySizeController(IBodySizeService bodySizeService, IMapper mapper)
        {
            this.bodySizeService = bodySizeService;
            this.mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            try
            {
                var bodySize = bodySizeService.GetBodySize(id);
                if (bodySize == null)
                {
                    return NotFound("không tìm thấy thuật ngữ số đo cơ thể này");
                }
                else
                {
                    return Ok(mapper.Map<BodySizeVM>(bodySize));
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

        [HttpGet]
        public IActionResult GetAll(string? search)
        {
            try
            {
                var bodySizes = bodySizeService.GetBodySizes(search);
                return Ok(mapper.Map<IEnumerable<BodySizeVM>>(bodySizes));
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

        [HttpPost]
        public IActionResult CreateBodySize([FromBody] BodySizeVM bodySizeVM)
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
                //    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                        if (string.IsNullOrWhiteSpace(bodySizeVM.Name))
                        {
                            throw new UserException("Nhập tên thuật ngữ số đo cơ thể");
                        }
                        else
                        {
                            return bodySizeService.CreateBodySize(mapper.Map<BodySize>(bodySizeVM)) ? Ok("Tạo mới thuật ngữ số đo cơ thể thành công") : BadRequest("Tạo mới thuật ngữ số đo cơ thể thất bại");
                        }
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

        [HttpPut("{id}")]
        public IActionResult UpdateBodySize(string? id, [FromBody] BodySizeVM bodySizeVM)
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
                //    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                //        if (string.IsNullOrWhiteSpace(discount.Name) || string.IsNullOrEmpty(discount.Code))
                //        {
                //            throw new UserException("Nhập tên phiếu giảm giá");
                //        }
                //        else
                //        {
                            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(bodySizeVM.Id) || id != bodySizeVM.Id)
                            {
                                return NotFound("Không tìm thấy loại nguyên liệu");
                            }
                            else
                            {
                                return bodySizeService.UpdateBodySize(mapper.Map<BodySize>(bodySizeVM)) ? Ok("Cập nhật thuật ngữ số đo cơ thể thành công") : BadRequest("Cập nhật thuật ngữ số đo cơ thể thất bại");
                            }
                //        }
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

        [HttpDelete("{id}")]
        public IActionResult DeleteBodySize(string? id)
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
                //    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (!staffService.CheckSecrectKey(staffId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                        if (string.IsNullOrEmpty(id))
                        {
                            return NotFound("Không tìm thấy thuật ngữ số đo cơ thể");
                        }
                        else
                        {
                            return bodySizeService.DeleteBodySize(id) ? Ok("Xóa thuật ngữ số đo cơ thể thành công") : BadRequest("Xóa thuật ngữ số đo cơ thể thất bại");
                        }
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
    }
}
