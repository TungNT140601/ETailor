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
        private readonly IAuthService authService;
        public AuthController(ICustomerService customerService, IConfiguration configuration, IStaffService staffService
            , IMapper mapper, IWebHostEnvironment webHost, IAuthService authService)
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
            this.authService = authService;
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
                    Name = customer.Fullname ?? "",
                    Avatar = customer.Avatar != string.Empty ? Ultils.GetUrlImage(customer.Avatar) : "",
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
        public async Task<IActionResult> VerifyEmail(string email)
        {
            try
            {
                if (Ultils.IsValidEmail(email)) //Check email valid
                {
                    var customer = customerService.FindEmail(email);
                    var otp = Ultils.GenerateRandomOTP();
                    if (customer == null)
                    {
                        var check = await customerService.CreateCustomer(new Customer()
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

                        return Ok("Đã gửi mail");
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

                        return Ok("Đã gửi mail");
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
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role != RoleName.CUSTOMER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    customerService.Logout(id);
                }

                return Ok("Đăng xuất thành công");
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
                    return Unauthorized("Chưa đăng nhập");
                }
                if (role != RoleName.CUSTOMER)
                {
                    return Unauthorized("Không có quyền truy cập");
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
        #endregion

        #region Staff
        [HttpPost("staff/login")]
        public async Task<IActionResult> CheckLoginStaff([FromBody] StaffLogin staffLogin)
        {
            try
            {
                var staff = staffService.CheckLogin(staffLogin.Username, staffLogin.Password);

                return Ok(new
                {
                    Role = staff.Role == 0 ? RoleName.ADMIN : staff.Role == 1 ? RoleName.MANAGER : RoleName.STAFF,
                    Name = staff.Fullname ?? string.Empty,
                    Avatar = staff.Avatar != string.Empty ? Ultils.GetUrlImage(staff.Avatar) : "",
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
                    return Unauthorized("Chưa đăng nhập");
                }
                else if (role == RoleName.CUSTOMER)
                {
                    return Unauthorized("Không có quyền truy cập");
                }
                else
                {
                    staffService.Logout(id);
                }

                return Ok("Đăng xuất thành công");
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

        #region MMA
        [HttpPost("mma/regis")]
        public async Task<IActionResult> CusMMARegis([FromForm] CusRegis cusRegis)
        {

            try
            {
                var check = await authService.Regis(cusRegis.Username, cusRegis.Email, cusRegis.Fullname, cusRegis.Phone, cusRegis.Address, cusRegis.Password, cusRegis.AvatarImage);
                return check ? Ok("Đăng ký tài khoản thành công") : BadRequest("Đăng ký tài khoản thất bại");
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
        [HttpPost("mma/login")]
        public async Task<IActionResult> MMALogin([FromBody] CusLoginEmail cusLoginEmail)
        {

            try
            {
                var customer = authService.CheckLoginCus(cusLoginEmail.EmailOrUsername, cusLoginEmail.Password);
                if (customer == null)
                {
                    var staff = authService.CheckLoginStaff(cusLoginEmail.EmailOrUsername, cusLoginEmail.Password);
                    if (staff == null)
                    {
                        throw new UserException("Sai thông tin đăng nhập");
                    }
                    else
                    {
                        return Ok(new
                        {
                            Name = staff.Fullname ?? string.Empty,
                            Avatar = staff.Avatar != string.Empty ? Ultils.GetUrlImage(staff.Avatar) : "",
                            Token = Ultils.GetToken(staff.Id, staff.Fullname, staff.Role == 0 ? RoleName.ADMIN : staff.Role == 1 ? RoleName.MANAGER : RoleName.STAFF, staff.SecrectKeyLogin, configuration),
                            Role = staff.Role == 0 ? RoleName.ADMIN : staff.Role == 1 ? RoleName.MANAGER : RoleName.STAFF
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        Name = customer.Fullname ?? "",
                        Avatar = customer.Avatar != string.Empty ? Ultils.GetUrlImage(customer.Avatar) : "",
                        Token = Ultils.GetToken(customer.Id, customer.Fullname ?? string.Empty, RoleName.CUSTOMER, customer.SecrectKeyLogin, configuration),
                        Role = RoleName.CUSTOMER
                    });
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
