using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity.PaymentConfig;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IOrderRepository orderRepository;
        public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository)
        {
            this.paymentRepository = paymentRepository;
            this.orderRepository = orderRepository;
        }

        public string CreatePayment(string orderId, decimal amount, int payType, string platform, string ip)
        {
            var order = orderRepository.Get(orderId);

            if (order == null || order.IsActive == false)
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
            else
            {
                if (payType == 2)
                {
                    if (amount > 0)
                    {
                        amount = 0 - amount;
                    }
                }
                else
                {
                    if (amount < 0)
                    {
                        amount = Math.Abs(amount);
                    }
                }

                var payment = new Payment()
                {
                    Id = Ultils.GenGuidString(),
                    OrderId = orderId,
                    CreatedTime = DateTime.Now,
                    Amount = amount,
                    Platform = platform,
                    PayTime = null,
                    PayType = payType,
                    Status = platform == PlatformName.OFFLINE ? 1 : 0
                };

                if (paymentRepository.Create(payment))
                {
                    if (platform == PlatformName.VN_PAY)
                    {
                        return GetVNPayUrlPayment(payment, ip);
                    }
                    if (platform == PlatformName.OFFLINE)
                    {
                        return payment.Id;
                    }

                    return null;
                }
                else
                {
                    return null;
                }

            }
        }

        public bool UpdatePayment(string paymentId, int status)
        {
            var payment = paymentRepository.Get(paymentId);
            if (payment != null)
            {
                payment.Status = status;
                payment.PayTime = DateTime.Now;

                if (paymentRepository.Update(paymentId, payment))
                {
                    if (payment.Status == 1)
                    {
                        switch (payment.PayType)
                        {
                            case 0:
                                {
                                    return true;
                                }
                            case 1:
                                {
                                    return true;
                                }
                            case 2:
                                {
                                    return true;
                                }
                            default:
                                {
                                    return true;
                                }
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<Payment> GetAllPayments(string? orderId)
        {
            return paymentRepository.GetAll(x => string.IsNullOrWhiteSpace(orderId) || orderId == x.OrderId);
        }

        private string GetVNPayUrlPayment(Payment payment, string ip)
        {
            var _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            //Get Config Info
            string vnp_Returnurl = _configuration["VNPayConfig:vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = _configuration["VNPayConfig:vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = _configuration["VNPayConfig:vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = _configuration["VNPayConfig:vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            if (payment.Amount < 0)
            {
                payment.Amount = Math.Abs(payment.Amount.Value);
            }
            var amout = Math.Round(payment.Amount.Value);
            vnpay.AddRequestData("vnp_Amount", (amout * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_BankCode", "");

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ip);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + payment.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", payment.Id); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return paymentUrl;
        }
    }
}
