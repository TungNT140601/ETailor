using AutoMapper;
using Etailor.API.Repository.EntityModels;
using Etailor.API.Service.Interface;
using Etailor.API.Service.Service;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity.PaymentConfig;
using Etailor.API.WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;
        private readonly ICustomerService customerService;
        private readonly IStaffService staffService;
        private readonly IConfiguration configuration;
        public PaymentController(IPaymentService paymentService, ICustomerService customerService, IConfiguration configuration, IStaffService staffService)
        {
            this.paymentService = paymentService;
            this.customerService = customerService;
            this.configuration = configuration;
            this.staffService = staffService;
        }
        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        [HttpPost("{orderId}")]
        public async Task<IActionResult> CreatePaymnent(string orderId, int? amount, int payType, string platform)
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
                        var result = await paymentService.CreatePayment(orderId, amount, payType, platform, GetIpAddress(), staffId);
                        if (result != null)
                        {
                            return Ok(result);
                        }
                        else
                        {
                            throw new UserException("Tạo thanh toán thất bại");
                        }
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

        [HttpPost("refund/{paymentId}")]
        public async Task<IActionResult> CreateRefund(string paymentId, int transactionType, decimal? amount)
        {
            try
            {
                //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                //if (role == null)
                //{
                //    return Unauthorized("Chưa đăng nhập");
                //}
                //else if (role != RoleName.CUSTOMER)
                //{
                //    return Unauthorized("Không có quyền truy cập");
                //}
                //else
                //{
                //    var customerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                //    var secrectKey = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.CookiePath)?.Value;
                //    if (string.IsNullOrEmpty(customerId) || !customerService.CheckSecerctKey(customerId, secrectKey))
                //    {
                //        return Unauthorized("Chưa đăng nhập");
                //    }
                //    else
                //    {
                var result = await paymentService.RefundMoneyVNPay(paymentId, transactionType, amount);

                if (result != null)
                {
                    //return result.Contains("https://") ? Redirect(result.ToString()) : Ok("Tạo thanh toán thành công");
                    return Ok(result);
                }
                else
                {
                    throw new UserException("Tạo thanh toán thất bại");
                }
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

        [HttpGet("result/vnp")]
        public async Task<IActionResult> GetVNPayPaymentResult()
        {
            try
            {
                var referer = Request.Headers["Referer"].ToString();

                if (string.IsNullOrWhiteSpace(referer) || !referer.StartsWith(configuration.GetSection("VNPayConfig:vnp_Url_Result").Value))
                {
                    return NotFound();
                }

                var vnpayData = Request.Query;

                var vnpLib = new VnPayLibrary();

                if (vnpayData.Count > 0)
                {
                    foreach (var s in vnpayData)
                    {
                        //get all querystring data
                        if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                        {
                            vnpLib.AddResponseData(s.Key, s.Value);
                        }
                    }
                }

                var vnp_HashSecret = configuration.GetSection("VNPayConfig:vnp_HashSecret").Value;

                var vnp_TxnRef = vnpLib.GetResponseData("vnp_TxnRef");

                string vnp_ResponseCode = vnpLib.GetResponseData("vnp_ResponseCode");

                string vnp_SecureHash = vnpLib.GetResponseData("vnp_SecureHash");

                if (vnpLib.ValidateSignature(vnp_SecureHash, vnp_HashSecret))
                {
                    var clientUrl = configuration.GetValue<string>("Client_Url");

                    if (await paymentService.UpdatePayment(vnp_TxnRef, vnp_ResponseCode != null && vnp_ResponseCode == "00" ? 0 : int.Parse(vnp_ResponseCode)))
                    {
                        return Redirect($"{clientUrl}/payment-success");
                    }
                    else
                    {
                        return Redirect($"{clientUrl}/payment-fail");
                    }
                }
                else
                {
                    throw new SystemsException("Validate Signature Fail", nameof(PaymentController));
                }

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
