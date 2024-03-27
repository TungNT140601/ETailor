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
        private readonly IStaffService staffService;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly string _wwwrootPath;

        public CustomerController(ICustomerService customerService, IConfiguration configuration, IMapper mapper, IWebHostEnvironment webHost, IStaffService staffService)
        {
            this.customerService = customerService;
            this.configuration = configuration;
            this.mapper = mapper;
            this._wwwrootPath = webHost.WebRootPath;
            this.staffService = staffService;
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
                    return Unauthorized("Không có quyền truy cập");
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
        [HttpGet]
        public async Task<IActionResult> GetCustomers(string? search)
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
                    var customerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (string.IsNullOrEmpty(customerId) || !staffService.CheckSecrectKey(customerId, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var customers = mapper.Map<IEnumerable<CustomerAllVM>>(customerService.FindPhoneOrEmail(search));
                        if (customers != null && customers.Any())
                        {
                            var tasks = new List<Task>();
                            foreach (var customer in customers)
                            {
                                tasks.Add(Task.Run(async () =>
                                {
                                    customer.Avatar = await Ultils.GetUrlImage(customer.Avatar);
                                }));
                            }
                            await Task.WhenAll(tasks);
                        }
                        return Ok(customers);
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
        [HttpGet("info/{id}")]
        public async Task<IActionResult> StaffGetCustomer(string id)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.MANAGER && role != RoleName.STAFF)
                //{
                //    return Unauthorized("Không có quyền truy cập");
                //}
                //else
                //{
                //    var customerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (string.IsNullOrEmpty(customerId) || !staffService.CheckSecerctKey(customerId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                var customer = mapper.Map<CustomerAllVM>(customerService.FindById(id));
                if (customer != null)
                {
                    customer.Avatar = await Ultils.GetUrlImage(customer.Avatar);
                }
                return Ok(customer);
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
        [HttpGet("info")]
        public async Task<IActionResult> GetInfo()
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
                    var customerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (string.IsNullOrEmpty(customerId) || !customerService.CheckSecerctKey(customerId, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        var customer = mapper.Map<CustomerAllVM>(customerService.FindById(customerId));
                        if (customer != null)
                        {
                            customer.Avatar = await Ultils.GetUrlImage(customer.Avatar);
                        }
                        return Ok(customer);
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
        [HttpPost("staff/customer")]
        public async Task<IActionResult> StaffCreateCustomer([FromBody] CustomerCreateVM cus)
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
                    var staffId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                    if (string.IsNullOrEmpty(staffId) || !staffService.CheckSecrectKey(staffId, secrectKey))
                    {
                        return Unauthorized("Chưa đăng nhập");
                    }
                    else
                    {
                        return await customerService.CreateCustomer(mapper.Map<Customer>(cus)) ? Ok("Tạo khách hàng thành công") : BadRequest("Tạo khách hàng thất bại");
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
