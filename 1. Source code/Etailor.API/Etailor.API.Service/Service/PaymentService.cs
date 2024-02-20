using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity.PaymentConfig;
using Google.Api.Gax;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderService orderService;
        public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IOrderService orderService)
        {
            this.paymentRepository = paymentRepository;
            this.orderRepository = orderRepository;
            this.orderService = orderService;
        }

        public string CreatePayment(string orderId, int? percent, int payType, string platform, string ip)
        {
            var order = orderRepository.Get(orderId);

            double amount = 0;

            if (order == null || order.IsActive == false)
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
            else
            {
                if (payType == 1 && percent.HasValue)
                {
                    amount = (double)order.TotalPrice * (double)percent / 100;
                }
                else if (payType == 0)
                {
                    amount = order.UnPaidMoney.HasValue ? (double)order.UnPaidMoney.Value : 0;
                }
                else
                {
                    throw new UserException("Loại thanh toán không phù hợp");
                }

                var payment = new Payment()
                {
                    Id = Ultils.GenGuidString(),
                    OrderId = orderId,
                    CreatedTime = DateTime.Now,
                    Amount = (decimal)Math.Round(amount, 2),
                    Platform = platform,
                    PayTime = null,
                    PayType = payType,
                    Status = platform == PlatformName.OFFLINE ? 0 : 1
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
                    if (payment.Status == 0)
                    {
                        switch (payment.PayType)
                        {
                            case 0:
                                {
                                    return orderService.CheckOrderPaid(payment.OrderId);
                                }
                            case 1:
                                {
                                    return orderService.PayDeposit(payment.OrderId, payment.Amount.GetValueOrDefault());
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

        //public bool RefundMoneyVNPay(string paymentId, int transactionType, decimal? amount)
        //{
        //    if (transactionType != 2 && transactionType != 3)
        //    {
        //        throw new UserException("Loại giao dịch không phù hợp");
        //    }

        //    var payment = paymentRepository.Get(paymentId);

        //    if (payment != null)
        //    {
        //        if (transactionType == 3 && !amount.HasValue)
        //        {
        //            throw new UserException("Vui lòng nhập số tiền hoàn một phần");
        //        }
        //        else if (transactionType == 3 && amount.HasValue)
        //        {
        //            if (amount.Value <= 0 || amount.Value >= (payment.Amount < 0 ? Math.Abs(payment.Amount.Value) : payment.Amount))
        //            {
        //                throw new UserException("Số tiền hoàn không hợp lệ");
        //            }
        //        }

        //        var paymentRefund = new Payment()
        //        {
        //            Id = Ultils.GenGuidString(),
        //            OrderId = payment.OrderId,
        //            CreatedTime = DateTime.Now,
        //            Amount = (decimal)Math.Round(transactionType == 3 ? 0 - amount.Value : 0 - Math.Abs(payment.Amount.Value), 2),
        //            Platform = PlatformName.VN_PAY,
        //            PayTime = null,
        //            PayType = 2,
        //            Status = 1
        //        };

        //        if (paymentRepository.Create(paymentRefund))
        //        {
        //            var body = GetVNPayUrlRefunt(paymentRefund.Id, transactionType, paymentId, payment.Amount.Value);
        //        }
        //    }
        //    else
        //    {
        //        throw new UserException("Không tìm thấy giao dịch");
        //    }
        //}

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
            var amout = Math.Round(payment.Amount.Value, 2);
            vnpay.AddRequestData("vnp_Amount", (amout * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_BankCode", "");

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ip);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang : " + payment.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", payment.Id); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return paymentUrl;
        }

        private SortedList<String, String> GetVNPayUrlRefunt(string id, int transactionType, string paymentRefuntId, decimal refuntAmount)
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

            var body = new SortedList<String, String>();

            var vnLib = new VnPayLibrary();

            body.Add("vnp_RequestId", id);

            body.Add("vnp_Version", VnPayLibrary.VERSION);

            body.Add("vnp_Command", "refund");

            body.Add("vnp_TmnCode", vnp_TmnCode);

            if (transactionType != 2 && transactionType != 3)
            {
                throw new UserException("Loại giao dịch không phù hợp");
            }

            body.Add("vnp_TransactionType", transactionType.ToString("00"));    //Loại giao dịch tại hệ thống VNPAY:
                                                                                //02: Giao dịch hoàn trả toàn phần(vnp_TransactionType= 02)
                                                                                //03: Giao dịch hoàn trả một phần(vnp_TransactionType= 03)

            body.Add("vnp_TxnRef", paymentRefuntId); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            if (refuntAmount < 0)
            {
                refuntAmount = Math.Abs(refuntAmount);
            }

            var amout = Math.Round(refuntAmount, 2);

            body.Add("vnp_Amount", (amout * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

            body.Add("vnp_OrderInfo", transactionType == 2 ? $"Giao dich hoan tra toan phan cua giao dich: {paymentRefuntId}" : transactionType == 3 ? $"Giao dich hoan tra mot phan cua giao dich: {paymentRefuntId}" : "Loi");

            body.Add("vnp_TransactionDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

            body.TryGetValue("vnp_TransactionDate", out string vnp_TransactionDate);

            body.Add("vnp_CreateBy", "ETailor");

            body.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

            body.TryGetValue("vnp_CreateDate", out string vnp_CreateDate);

            body.Add("vnp_IpAddr", "20.212.64.6");

            body.TryGetValue("vnp_IpAddr", out string vnp_IpAddr);

            var data = id + "|" + VnPayLibrary.VERSION + "|" + "refund" + "|" + vnp_TmnCode + "|" + transactionType.ToString("00") + "|" + paymentRefuntId + "|" + (amout * 100).ToString() + "|" + vnp_TransactionDate + "|" + "ETailor" + "|" + vnp_CreateDate + "|" + vnp_IpAddr + "|" + (transactionType == 2 ? $"Giao dich hoan tra toan phan cua giao dich: {paymentRefuntId}" : transactionType == 3 ? $"Giao dich hoan tra mot phan cua giao dich: {paymentRefuntId}" : "Loi");

            string vnp_SecureHash = Ultils.HmacSHA512(vnp_HashSecret, data);

            body.Add("vnp_SecureHash", vnp_SecureHash);

            return body;
        }
    }
}
