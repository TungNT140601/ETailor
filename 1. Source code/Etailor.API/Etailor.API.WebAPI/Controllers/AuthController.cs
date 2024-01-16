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
        private readonly IConfiguration configuration;
        public AuthController(ICustomerService customerService, IConfiguration configuration)
        {
            this.customerService = customerService;
            this.configuration = configuration;
        }

        [HttpPost("customer/login")]
        public IActionResult CustomerLoginEmail([FromBody] CusLoginEmail loginEmail)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(loginEmail.Email))
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
        //[HttpGet("customer/verify-phone")]
        //public IActionResult VerifyPhone(string email)
        //{
        //    try
        //    {
        //        if (Ultils.IsValidEmail(email))
        //        {
        //            var customer = customerService.FindEmail(email);
        //            var otp = Ultils.GenerateRandom6Digits();
        //            if (customer == null)
        //            {
        //                var check = customerService.CreateCustomer(new Customer()
        //                {
        //                    Email = email,
        //                    Otp = otp,
        //                    OtpexpireTime = DateTime.Now.AddMinutes(5),
        //                    Otpused = false
        //                });

        //                if (check)
        //                {
        //                    Ultils.SendOTPMail(email, otp);
        //                }

        //                return Ok();
        //            }
        //            else
        //            {
        //                var check = customerService.UpdateCustomerEmail(new Customer()
        //                {
        //                    Id = customer.Id,
        //                    Email = email,
        //                    Otp = otp,
        //                    OtpexpireTime = DateTime.Now.AddMinutes(5),
        //                    Otpused = false
        //                });

        //                if (check)
        //                {
        //                    Ultils.SendOTPMail(email, otp);
        //                }

        //                return Ok();
        //            }
        //        }
        //        else
        //        {
        //            throw new UserException("Email không đúng định dạng!!!");
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

        //[HttpPost("customer/verify-otp")]
        //public IActionResult VerifyOtp([FromBody] VerifyOtp verifyOtp)
        //{
        //    try
        //    {
        //        if (Ultils.IsValidEmail(verifyOtp.PhoneOrEmail))
        //        {
        //            var customer = customerService.FindEmail(verifyOtp.PhoneOrEmail);
        //            var otp = Ultils.GenerateRandom6Digits();
        //            if (customer == null)
        //            {
        //                throw new UserException("Email không có trong hệ thống");
        //            }
        //            else
        //            {
        //                if(customer.Otp == verifyOtp.Otp && customer.OtpexpireTime > DateTime.Now && customer.Otpused == false)
        //                {
        //                    var check = customerService.UpdateCustomerEmail(new Customer()
        //                    {
        //                        Id = customer.Id,
        //                        Email = email,
        //                        Otp = otp,
        //                        OtpexpireTime = DateTime.Now.AddMinutes(5),
        //                        Otpused = false
        //                    });
        //                }
                        

        //                if (check)
        //                {
        //                    Ultils.SendOTPMail(email, otp);
        //                }

        //                return Ok();
        //            }
        //        }
        //        else
        //        {
        //            throw new UserException("Email không đúng định dạng!!!");
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
    }
}
