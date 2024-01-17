using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ICustomerService customerService;
        private readonly IStaffService staffService;
        private readonly IConfiguration configuration;
        public AuthController(ICustomerService customerService, IConfiguration configuration, IStaffService staffService)
        {
            this.customerService = customerService;
            this.configuration = configuration;
            this.staffService = staffService;
        }

        [HttpPost("customer/login")]
        public IActionResult CustomerLoginEmail([FromBody] CusLoginEmail loginEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginEmail.Email))
                {
                    throw new UserException("Không được để trống EMAIL");
                }
                var customer = new Customer();
                if (Ultils.IsValidEmail(loginEmail.Email))
                {
                    customer = customerService.LoginWithEmail(loginEmail.Email, loginEmail.Password);
                }
                else
                {
                    customer = customerService.LoginWithUsername(loginEmail.Email, loginEmail.Password);
                }

                return Ok(new
                {
                    Token = Ultils.GetToken(customer.Id, customer.Fullname, RoleName.CUSTOMER, configuration)
                });
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
        [HttpGet("customer/verify-email")]
        public IActionResult VerifyEmail(string email)
        {
            try
            {
                if (Ultils.IsValidEmail(email))
                {
                    var customer = customerService.FindEmail(email);
                    var otp = Ultils.GenerateRandom6Digits();
                    if (customer == null)
                    {
                        var check = customerService.CreateCustomer(new Customer()
                        {
                            Email = email,
                            Otp = otp,
                            OtpexpireTime = DateTime.Now.AddMinutes(5),
                            Otpused = false
                        });

                        if (check)
                        {
                            Ultils.SendOTPMail(email, otp);
                        }

                        return Ok();
                    }
                    else
                    {
                        var check = customerService.UpdateCustomerEmail(new Customer()
                        {
                            Id = customer.Id,
                            Email = email,
                            Otp = otp,
                            OtpexpireTime = DateTime.Now.AddMinutes(5),
                            Otpused = false
                        });

                        if (check)
                        {
                            Ultils.SendOTPMail(email, otp);
                        }

                        return Ok();
                    }
                }
                else
                {
                    throw new UserException("Email không đúng định dạng!!!");
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

        [HttpGet("customer/verify-phone")]
        public IActionResult VerifyPhone(string phone)
        {
            try
            {
                if (Ultils.IsValidVietnamesePhoneNumber(phone))
                {
                    var customer = customerService.FindPhone(phone);
                    var otp = Ultils.GenerateRandom6Digits();
                    if (customer == null)
                    {
                        //var check = customerService.CreateCustomer(new Customer()
                        //{
                        //    Phone = phone,
                        //    Otp = otp,
                        //    OtpexpireTime = DateTime.Now.AddMinutes(5),
                        //    Otpused = false
                        //});

                        //if (check)
                        //{
                        //    Ultils.SendOTPPhone(phone, otp);
                        //}

                        return Ok();
                    }
                    else
                    {
                        //var check = customerService.UpdateCustomerEmail(new Customer()
                        //{
                        //    Id = customer.Id,
                        //    Email = email,
                        //    Otp = otp,
                        //    OtpexpireTime = DateTime.Now.AddMinutes(5),
                        //    Otpused = false
                        //});

                        //if (check)
                        //{
                        //    Ultils.SendOTPMail(email, otp);
                        //}

                        return Ok();
                    }
                }
                else
                {
                    throw new UserException("Số điện thoại không đúng định dạng!!!");
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

        [HttpPost("customer/verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtp verifyOtp)
        {
            try
            {
                return customerService.CheckOTP(verifyOtp.PhoneOrEmail, verifyOtp.Otp) ? Ok() : BadRequest("Sai chỗ nào rồi đó :))))");
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

        [HttpPost("staff/login")]
        public IActionResult CheckLoginStaff([FromBody] StaffLogin staffLogin)
        {
            try
            {
                var staff = staffService.CheckLogin(staffLogin.Username, staffLogin.Password);

                return Ok(new
                {
                    Staff = staff,
                    Token = Ultils.GetToken(staff.Id, staff.Fullname, RoleName.STAFF, configuration)
                });
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

        [HttpPost("staff/add")]
        public IActionResult AddStaff([FromBody] Staff staff)
        {
            try
            {
                return staffService.AddNewStaff(staff) ? Ok() : BadRequest();
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
