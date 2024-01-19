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

        public CustomerController(ICustomerService customerService, IConfiguration configuration, IMapper mapper)
        {
            this.customerService = customerService;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        [HttpPut("managed-personal-profile")]
        public IActionResult UpdatePersonalProfile([FromBody] CustomerVM customerVM)
        {
            try
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (role == null)
                {
                    return Unauthorized();
                }
                else if (role != RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                else
                {
                    var customerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (!customerService.CheckSecerctKey(customerId, secrectKey))
                    {
                        return Unauthorized();
                    }
                    else if (!string.IsNullOrEmpty(customerId))
                    {
                        var userInfo = customerService.FindById(customerId);
                        if (userInfo == null)
                        {
                            return Ok(new
                            {
                                Status = 0,
                                Message = "Tài khoản không có trong hệ thống!!!"
                            });
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(customerVM.Fullname))
                            {
                                return Ok(new
                                {
                                    Status = 0,
                                    Message = "Vui lòng nhập tên"
                                });
                            }
                            else
                            {
                                userInfo.Fullname = customerVM.Fullname;
                            }

                            if (string.IsNullOrEmpty(customerVM.Address))
                            {
                                return Ok(new
                                {
                                    Status = 0,
                                    Message = "Vui lòng nhập địa chỉ"
                                });
                            }
                            else
                            {
                                userInfo.Address = customerVM.Address;
                            }
                            return customerService.UpdatePersonalProfileCustomer(userInfo) ? Ok(new
                            {
                                Status = 1,
                                Message = "Success"
                            }) : Ok(new
                            {
                                Status = 0,
                                Message = "Fail"
                            });
                        }
                    }
                    else
                    {
                        throw new Exception("Something went wrong");
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
