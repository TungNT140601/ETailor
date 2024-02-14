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
    [Route("api/discount")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService discountService;
        private readonly IStaffService staffService;
        private readonly IMapper mapper;

        public DiscountController(IDiscountService discountService, IStaffService staffService, IMapper mapper)
        {
            this.mapper = mapper;
            this.discountService = discountService;
            this.staffService = staffService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            try
            {
                var discount = discountService.GetDiscount(id);
                if (discount == null)
                {
                    return NotFound("không tìm thấy loại giảm giá");
                }
                else
                {
                    return Ok(mapper.Map<DiscountVM>(discount));
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
                var discounts = discountService.GetDiscounts(search);
                return Ok(mapper.Map<IEnumerable<DiscountVM>>(discounts));
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
        public async Task<IActionResult> CreateDiscount([FromBody] DiscountCreateVM discount)
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
                return await discountService.CreateDiscount(mapper.Map<Discount>(discount)) ? Ok("Tạo mới phiếu giảm giá thành công") : BadRequest("Tạo mới phiếu giảm giá thất bại");
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
        public async Task<IActionResult> UpdateDiscount(string? id, [FromBody] DiscountVM discount)
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
                return await discountService.UpdateDiscount(mapper.Map<Discount>(discount)) ? Ok("Cập nhật phiếu giảm giá thành công") : BadRequest("Cập nhật phiếu giảm giá thất bại");
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
        public async Task<IActionResult> DeleteMaterialType(string? id)
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
                return discountService.DeleteDiscount(id) ? Ok("Xóa phiếu giảm giá thành công") : BadRequest("Xóa phiếu giảm giá thất bại");
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
