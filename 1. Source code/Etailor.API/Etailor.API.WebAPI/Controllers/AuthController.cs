using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.WebAPI.ViewModels;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
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
        private readonly StorageClient _storage;
        private readonly string _wwwrootPath;
        public AuthController(ICustomerService customerService, IConfiguration configuration, IStaffService staffService, IMapper mapper, IWebHostEnvironment webHost)
        {
            this.customerService = customerService;
            this.configuration = configuration;
            this.staffService = staffService;
            this.mapper = mapper;
            // Load Firebase credentials
            var credential = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), AppValue.FIREBASE_KEY));

            // Initialize StorageClient with Firebase credentials
            _storage = StorageClient.Create(credential);

            _wwwrootPath = webHost.WebRootPath;
        }

        #region Customer
        [HttpPost("customer/login")]
        public async Task<IActionResult> CustomerLoginEmail([FromBody] CusLoginEmail loginEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loginEmail.EmailOrUsername))
                {
                    throw new UserException("Không được để trống Email");
                }
                if (string.IsNullOrEmpty(loginEmail.ClientToken))
                {
                    loginEmail.ClientToken = string.Empty;
                }
                var customer = await customerService.Login(loginEmail.EmailOrUsername, loginEmail.Password, HttpContext.Connection.RemoteIpAddress.ToString(), loginEmail.ClientToken);

                return Ok(new
                {
                    Token = Ultils.GetToken(customer.Id, customer.Fullname ?? string.Empty, RoleName.CUSTOMER, customer.SecrectKeyLogin, configuration)
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
                            OtptimeLimit = DateTime.UtcNow.AddMinutes(5),
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
        public async Task<IActionResult> CustomerRegis([FromBody] CusRegis cus)
        {
            try
            {
                var customer = mapper.Map<Customer>(cus);
                customer.Avatar = await Ultils.UploadImage(_storage, _wwwrootPath, "CustomerAvatar", cus.AvatarImage);
                return customerService.CusRegis(customer) ? Ok("Đăng ký thành công") : BadRequest("Đăng ký thất bại");
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
                    Role = staff.Role == 0 ? RoleName.ADMIN : staff.Role == 1 ? RoleName.MANAGER : RoleName.STAFF,
                    Name = staff.Fullname ?? string.Empty,
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

        #endregion
    }
}
