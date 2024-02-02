using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Etailor.API.Service.Service;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/customer-management")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService customerService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly string _wwwrootPath;

        public CustomerController(ICustomerService customerService, IConfiguration configuration, IMapper mapper, IWebHostEnvironment webHost)
        {
            this.customerService = customerService;
            this.configuration = configuration;
            this.mapper = mapper;
            this._wwwrootPath = webHost.WebRootPath;
        }

        [HttpPut()]
        public async Task<IActionResult> UpdatePersonalProfile([FromForm] CustomerVM customerVM)
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
                    var customerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (string.IsNullOrEmpty(customerId) || !customerService.CheckSecerctKey(customerId, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var cus = mapper.Map<Customer>(customerVM);
                        cus.Id = customerId;
                        return (await customerService.UpdatePersonalProfileCustomer(cus, customerVM.AvatarImage, _wwwrootPath)) ? Ok() : BadRequest();
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

        [HttpPost("regis")]
        public async Task<IActionResult> CustomerRegis([FromForm] CusRegis cus)
        {
            try
            {
                return (await customerService.CusRegis(mapper.Map<Customer>(cus), cus.AvatarImage, _wwwrootPath)) ? Ok("Đăng ký thành công") : BadRequest("Đăng ký thất bại");
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
