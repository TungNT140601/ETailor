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
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;
        private readonly ICustomerService customerService;

        public PaymentController(IPaymentService paymentService, ICustomerService customerService)
        {
            this.paymentService = paymentService;
            this.customerService = customerService;
        }
        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        [HttpPost("{orderId}")]
        public async Task<IActionResult> CreatePaymnent(string orderId, int? persent, int payType, string platform)
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
                //    return Forbid("Không có quyền truy cập");
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
                var result = paymentService.CreatePayment(orderId, persent, payType, platform, GetIpAddress());
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
                //    return Forbid("Không có quyền truy cập");
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
        public IActionResult GetVNPayPaymentResult(string? vnp_TmnCode, string? vnp_Amount
            , string? vnp_BankCode, string? vnp_BankTranNo, string? vnp_CardType, string? vnp_PayDate
            , string? vnp_OrderInfo, string? vnp_TransactionNo, string? vnp_ResponseCode, string? vnp_TransactionStatus
            , string? vnp_TxnRef, string? vnp_SecureHashType, string? vnp_SecureHash)
        {
            var responseData = new
            {
                vnp_TmnCode,
                vnp_Amount,
                vnp_BankCode,
                vnp_BankTranNo,
                vnp_CardType,
                vnp_PayDate,
                vnp_OrderInfo,
                vnp_TransactionNo,
                vnp_ResponseCode,
                vnp_TransactionStatus,
                vnp_TxnRef,
                vnp_SecureHashType,
                vnp_SecureHash
            };

            paymentService.UpdatePayment(vnp_TxnRef, vnp_ResponseCode != null && vnp_ResponseCode == "00" ? 0 : int.Parse(vnp_ResponseCode));

            return Ok(responseData);
        }
    }
}
