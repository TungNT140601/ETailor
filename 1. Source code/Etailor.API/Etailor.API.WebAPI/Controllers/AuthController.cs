using AutoMapper;
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
        private readonly IMapper mapper;
        public AuthController(ICustomerService customerService, IConfiguration configuration, IStaffService staffService, IMapper mapper)
        {
            this.customerService = customerService;
            this.configuration = configuration;
            this.staffService = staffService;
            this.mapper = mapper;
        }

        #region Customer
        [HttpPost("customer/login")]
        public IActionResult CustomerLoginEmail([FromBody] CusLoginEmail loginEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginEmail.Email))
                {
                    throw new UserException("Không được để trống Email");
                }

                var customer = new Customer();

                if (Ultils.IsValidEmail(loginEmail.Email)) //Check email valid
                {
                    customer = customerService.LoginWithEmail(loginEmail.Email, loginEmail.Password); //call func LoginWithEmail from service
                }
                else
                {
                    customer = customerService.LoginWithUsername(loginEmail.Email, loginEmail.Password); //call func LoginWithUsername from service
                }

                return Ok(new
                {
                    Token = Ultils.GetToken(customer.Id, customer.Fullname, RoleName.CUSTOMER, customer.SecrectKeyLogin, configuration)
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
                if (Ultils.IsValidEmail(email)) //Check email valid
                {
                    var customer = customerService.FindEmail(email);
                    var otp = Ultils.GenerateRandom6Digits();
                    if (customer == null)
                    {
                        var check = customerService.CreateCustomer(new Customer()
                        {
                            Email = email,
                            EmailVerified = false,
                            Otpnumber = otp,
                            OtptimeLimit = DateTime.Now.AddMinutes(5),
                            Otpused = false,
                            IsActive = true
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
                            EmailVerified = false,
                            Otpnumber = otp,
                            OtptimeLimit = DateTime.Now.AddMinutes(5),
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

        //[HttpGet("customer/verify-phone")]
        //public IActionResult VerifyPhone(string phone)
        //{
        //    try
        //    {
        //        if (Ultils.IsValidVietnamesePhoneNumber(phone))
        //        {
        //            var customer = customerService.FindPhone(phone);
        //            var otp = Ultils.GenerateRandom6Digits();
        //            if (customer == null)
        //            {
        //                //var check = customerService.CreateCustomer(new Customer()
        //                //{
        //                //    Phone = phone,
        //                //    Otp = otp,
        //                //    OtpexpireTime = DateTime.Now.AddMinutes(5),
        //                //    Otpused = false
        //                //});

        //                //if (check)
        //                //{
        //                //    Ultils.SendOTPPhone(phone, otp);
        //                //}

        //                return Ok();
        //            }
        //            else
        //            {
        //                //var check = customerService.UpdateCustomerEmail(new Customer()
        //                //{
        //                //    Id = customer.Id,
        //                //    Email = email,
        //                //    Otp = otp,
        //                //    OtpexpireTime = DateTime.Now.AddMinutes(5),
        //                //    Otpused = false
        //                //});

        //                //if (check)
        //                //{
        //                //    Ultils.SendOTPMail(email, otp);
        //                //}

        //                return Ok();
        //            }
        //        }
        //        else
        //        {
        //            throw new UserException("Số điện thoại không đúng định dạng!!!");
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

        [HttpPost("customer/logout")]
        public IActionResult CusLogout()
        {
            try
            {
                var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                if (id == null)
                {
                    return Unauthorized();
                }
                else if (role != RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                else
                {
                    customerService.Logout(id);
                }

                return Ok();
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

        [HttpPost("customer/change-password")]
        public IActionResult CusChangePass([FromBody] ChangePassModel changePassModel)
        {
            try
            {
                var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                if (id == null || !customerService.CheckSecerctKey(id, secrectKey))
                {
                    return Unauthorized();
                }
                if (role != RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                return customerService.ChangePassword(id, changePassModel.OldPassword, changePassModel.NewPassword) ? Ok("Đổi mật khẩu thành công!!!") : BadRequest("Đổi mật khẩu thất bại");
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

        [HttpPost("customer/reset-password/{email}")]
        public IActionResult CusResetPass(string email)
        {
            try
            {
                return customerService.ResetPassword(email) ? Ok("Đã gửi mail") : BadRequest("Thất bại");
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

        [HttpPost("customer/regis")]
        public IActionResult CustomerRegis([FromBody] CusRegis cus)
        {
            try
            {
                return customerService.CusRegis(mapper.Map<Customer>(cus)) ? Ok("Đăng ký thành công") : BadRequest("Đăng ký thất bại");
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
        #endregion

        #region Staff
        [HttpPost("staff/login")]
        public IActionResult CheckLoginStaff([FromBody] StaffLogin staffLogin)
        {
            try
            {
                var staff = staffService.CheckLogin(staffLogin.Username, staffLogin.Password);

                return Ok(new
                {
                    Time = DateTime.Now.ToLongTimeString(),
                    Staff = mapper.Map<StaffVM>(staff),
                    Role = staff.Role == 0 ? RoleName.ADMIN : staff.Role == 1 ? RoleName.MANAGER : RoleName.STAFF,
                    Token = Ultils.GetToken(staff.Id, staff.Fullname, staff.Role == 0 ? RoleName.ADMIN : staff.Role == 1 ? RoleName.MANAGER : RoleName.STAFF, staff.SecrectKeyLogin, configuration)
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
        public IActionResult AddStaff([FromBody] StaffVM staff)
        {
            try
            {
                return staffService.AddNewStaff(mapper.Map<Staff>(staff)) ? Ok() : BadRequest();
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

        [HttpPost("staff/logout")]
        public IActionResult StaffLogout()
        {
            try
            {
                var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                if (id == null)
                {
                    return Unauthorized();
                }
                else if (role == RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                else
                {
                    staffService.Logout(id);
                }

                return Ok();
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

        [HttpGet("staff/get-info")]
        public IActionResult StaffInfo()
        {
            try
            {
                var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                if (id == null)
                {
                    return Unauthorized();
                }
                else if (role == RoleName.CUSTOMER)
                {
                    return Forbid();
                }
                else
                {
                    staffService.Logout(id);
                }
                var expirationClaim = User.FindFirst("exp");

                if (expirationClaim != null && long.TryParse(expirationClaim.Value, out long exp))
                {
                    DateTime expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;


                    return Ok(new
                    {
                        ExpirationClaim = expirationTime.ToLongTimeString(),
                        Time = DateTime.Now.ToLongTimeString(),
                        Data = mapper.Map<StaffVM>(staffService.GetStaff(id)),
                    });
                }
                else
                {
                    // Unable to retrieve expiration claim or parse expiration time
                    return Unauthorized("Invalid JWT or missing expiration claim.");
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
        #endregion
    }
}
