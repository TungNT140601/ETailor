using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/order-material")]
    [ApiController]
    public class OrderMaterialController : ControllerBase
    {
        private readonly IStaffService staffService;
        private readonly ICustomerService customerService;
        private readonly IOrderMaterialService orderMaterialService;
        private readonly IProductService productService;
        private readonly IProductTemplateService productTemplateService;
        private readonly IDiscountService discountService;
        private readonly IMapper mapper;

        public OrderMaterialController(IStaffService staffService, ICustomerService customerService,
            IOrderMaterialService orderMaterialService, IMapper mapper, IProductService productService, IProductTemplateService productTemplateService,
            IDiscountService discountService)
        {
            this.mapper = mapper;
            this.staffService = staffService;
            this.customerService = customerService;
            this.orderMaterialService = orderMaterialService;
            this.productService = productService;
            this.productTemplateService = productTemplateService;
            this.discountService = discountService;
        }
        [HttpPut("/order/{id}")]
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] List<OrderMaterialUpdateVM>? orderMaterialVMs)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.MANAGER && role != RoleName.STAFF)
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
                        var check = await orderMaterialService.UpdateOrderMaterials(id, mapper.Map<List<OrderMaterial>>(orderMaterialVMs));

                        return check ? Ok("Cập nhật nguyên liệu hóa đơn thành công") : BadRequest("Cập nhật nguyên liệu hóa đơn thất bại");
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
