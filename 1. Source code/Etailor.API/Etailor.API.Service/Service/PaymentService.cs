using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Etailor.API.Ultity.PaymentConfig;
using Google.Api.Gax;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderService orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISignalRService signalRService;
        public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IOrderService orderService, IHttpContextAccessor httpContextAccessor, ISignalRService signalRService)
        {
            this.paymentRepository = paymentRepository;
            this.orderRepository = orderRepository;
            this.orderService = orderService;
            this._httpContextAccessor = httpContextAccessor;
            this.signalRService = signalRService;
        }

        public async Task<string> CreatePayment(string orderId, decimal? amount, int payType, string platform, string ip, string createrId)
        {
            var order = orderRepository.Get(orderId);

            if (order == null)
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
            else
            {
                if (payType == 1)
                {
                    if (paymentRepository.GetAll(x => x.OrderId == order.Id && x.PayType == 1 && x.Status == 0).Any())
                    {
                        throw new UserException("Hóa đơn này đã được thanh toán cọc.");
                    }
                    else if (amount.HasValue)
                    {
                        if (amount < 5000 || (order.DiscountId != null && amount.Value > order.AfterDiscountPrice) || (order.DiscountId == null && amount.Value > order.TotalPrice))
                        {
                            throw new UserException("Số tiền cọc không hợp lệ.");
                        }
                    }
                    else
                    {
                        if (order.AfterDiscountPrice.HasValue && order.AfterDiscountPrice.Value > 0)
                        {
                            amount = Math.Round(((order.AfterDiscountPrice.Value * (decimal)0.3) / 1000), 0) * 1000;
                        }
                        else
                        {
                            amount = Math.Round(((order.TotalPrice.Value * (decimal)0.3) / 1000), 0) * 1000;
                        }
                    }
                }
                else if (payType == 0)
                {
                    amount = order.UnPaidMoney.HasValue ? (decimal)order.UnPaidMoney.Value : 0;
                }
                else
                {
                    throw new UserException("Loại thanh toán không phù hợp");
                }

                var payment = new Payment()
                {
                    Id = Ultils.GenGuidString(),
                    OrderId = orderId,
                    CreatedTime = DateTime.UtcNow.AddHours(7),
                    Amount = (decimal)Math.Round(amount.Value, 2),
                    AmountAfterRefund = (decimal)Math.Round(amount.Value, 2),
                    PaymentRefundId = null,
                    Platform = platform,
                    PayTime = null,
                    PayType = payType,
                    StaffCreateId = createrId,
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
                        await orderService.CheckOrderPaid(orderId);
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

        public async Task<bool> UpdatePayment(string paymentId, int status)
        {
            var payment = paymentRepository.Get(paymentId);
            if (payment != null)
            {
                payment.Status = status;
                payment.PayTime = DateTime.UtcNow.AddHours(7);

                if (paymentRepository.Update(paymentId, payment))
                {
                    if (payment.Status == 0)
                    {
                        switch (payment.PayType)
                        {
                            case 0:
                                {
                                    if (await orderService.CheckOrderPaid(payment.OrderId))
                                    {
                                        if (payment.Platform == PlatformName.VN_PAY)
                                        {
                                            await signalRService.SendVNPayResult("True");
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        if (payment.Platform == PlatformName.VN_PAY)
                                        {
                                            await signalRService.SendVNPayResult("False");
                                        }
                                        return false;
                                    }
                                }
                            case 1:
                                {
                                    if (await orderService.PayDeposit(payment.OrderId, payment.Amount.GetValueOrDefault()))
                                    {
                                        if (payment.Platform == PlatformName.VN_PAY)
                                        {
                                            await signalRService.SendVNPayResult("True");
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        if (payment.Platform == PlatformName.VN_PAY)
                                        {
                                            await signalRService.SendVNPayResult("False");
                                        }
                                        return false;
                                    }
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
                }
            }
            if (payment.Platform == PlatformName.VN_PAY)
            {
                await signalRService.SendVNPayResult("False");
            }
            return false;
        }

        public async Task<bool> RefundMoneyVNPay(string paymentId, int transactionType, decimal? amount)
        {
            if (transactionType != 2 && transactionType != 3)
            {
                throw new UserException("Loại giao dịch không phù hợp");
            }

            var payment = paymentRepository.Get(paymentId);

            if (payment != null)
            {
                if (transactionType == 3 && !amount.HasValue)
                {
                    throw new UserException("Vui lòng nhập số tiền hoàn một phần");
                }
                else if (transactionType == 3 && amount.HasValue)
                {
                    if (amount.Value <= 0 || amount.Value >= (payment.Amount < 0 ? Math.Abs(payment.Amount.Value) : payment.Amount))
                    {
                        throw new UserException("Số tiền hoàn không hợp lệ");
                    }
                }

                var paymentRefund = new Payment()
                {
                    Id = Ultils.GenGuidString(),
                    OrderId = payment.OrderId,
                    CreatedTime = DateTime.UtcNow.AddHours(7),
                    Amount = (decimal)Math.Round(transactionType == 3 ? 0 - amount.Value : 0 - Math.Abs(payment.Amount.Value), 2),
                    PaymentRefundId = payment.Id,
                    Platform = PlatformName.VN_PAY,
                    PayTime = null,
                    PayType = 2,
                    Status = 1
                };

                payment.AmountAfterRefund = payment.Amount + paymentRefund.Amount;

                if (paymentRepository.Create(paymentRefund))
                {
                    var body = GetVNPayUrlRefunt(paymentRefund.Id, transactionType, paymentId, paymentRefund.Amount.Value);

                    HttpClient client = new HttpClient();
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("https://sandbox.vnpayment.vn/merchant_webapi/api/transaction", content);
                    string res = await response.Content.ReadAsStringAsync();

                    var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (json["vnp_ResponseCode"] == "00")
                        {
                            paymentRefund.Status = 0;
                            paymentRefund.PayTime = DateTime.UtcNow.AddHours(7);

                            return paymentRepository.Update(payment.Id, payment) && paymentRepository.Update(paymentRefund.Id, paymentRefund);
                        }
                        else
                        {
                            var result = "";
                            foreach (var key in json.Keys)
                            {
                                json.TryGetValue(key, out string value);
                                result = result + " | " + key + ": " + value;
                            }
                            throw new UserException("Giao dịch hoàn tiền thất bại: " + result);
                        }
                    }
                    else
                    {
                        throw new UserException("Gửi yêu cầu thất bại: " + response.StatusCode);
                    }
                }
                else
                {
                    throw new UserException("Tạo giao dịch hoàn tiền thất bại");
                }
            }
            else
            {
                throw new UserException("Không tìm thấy giao dịch");
            }
        }

        public async Task<bool> RefundMoney(string orderId, decimal amount, string createrId)
        {
            var order = orderRepository.Get(orderId);
            if (order != null && order.IsActive == true)
            {
                switch (order.Status)
                {
                    case 0:
                        throw new UserException("Hóa đơn đã hủy");
                    case 1:
                        throw new UserException("Hóa đơn chưa được xác nhận");
                    case 5:
                        throw new UserException("Hóa đơn đã hoàn thành các sản phẩm, không thể hoàn tiền");
                    case 6:
                        throw new UserException("Hóa đơn đang chờ khách kiểm duyệt, không thể hoàn tiền");
                    case 7:
                        throw new UserException("Hóa đơn bị từ chối, không thể hoàn tiền");
                    case 8:
                        throw new UserException("Hóa đơn hoàn tất, không thể hoàn tiền");
                }
                if (amount <= 0)
                {
                    throw new UserException("Số tiền hoàn không hợp lệ");
                }
                else if (order.PaidMoney == 0)
                {
                    throw new UserException("Hóa đơn chưa được thanh toán");
                }
                else if (order.PaidMoney < amount)
                {
                    throw new UserException("Số tiền hoàn lớn hơn số tiền đã thanh toán");
                }
                else
                {
                    var payment = new Payment()
                    {
                        Id = Ultils.GenGuidString(),
                        Amount = 0 - amount,
                        AmountAfterRefund = order.PaidMoney - amount,
                        CreatedTime = DateTime.UtcNow.AddHours(7),
                        OrderId = orderId,
                        PaymentRefundId = null,
                        PayTime = DateTime.UtcNow.AddHours(7),
                        PayType = 2,
                        Platform = PlatformName.OFFLINE,
                        StaffCreateId = createrId,
                        Status = 0
                    };

                    if (paymentRepository.Create(payment))
                    {
                        try
                        {
                            var result = await orderRepository.GetStoreProcedureReturnInt(StoreProcName.Cancel_Order,
                                new SqlParameter
                                {
                                    DbType = System.Data.DbType.String,
                                    Value = orderId,
                                    ParameterName = "@OrderId"
                                });
                            if (result == 1)
                            {
                                return true;
                            }
                            else
                            {
                                throw new SystemsException("Lỗi trong quá trình hủy hóa đơn", nameof(OrderService.DeleteOrder));
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new UserException(ex.Message);
                        }
                    }
                    else
                    {
                        throw new SystemsException("Tạo giao dịch hoàn tiền thất bại", nameof(PaymentService.RefundMoney));
                    }
                }
            }
            else
            {
                throw new UserException("Không tìm thấy hóa đơn");
            }
        }

        public IEnumerable<Payment> GetAllOrderPayments(string? orderId)
        {
            return paymentRepository.GetAll(x => (string.IsNullOrWhiteSpace(orderId) || orderId == x.OrderId) && x.Status == 0);
        }
        public IEnumerable<Payment> GetAllPayments()
        {
            return paymentRepository.GetAll(x => x.Status == 0);
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
            var amout = Math.Round(payment.Amount.Value, 2) * 100;

            vnpay.AddRequestData("vnp_Amount", ((int)amout).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_BankCode", "");

            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ip);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang : " + payment.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            string scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            string host = _httpContextAccessor.HttpContext.Request.Host.Host;
            int? port = _httpContextAccessor.HttpContext.Request.Host.Port;

            string fullUrl = $"{scheme}://{host}";

            if (port.HasValue && port != 80 && port != 443)
            {
                fullUrl += $":{port}";
            }

            vnpay.AddRequestData("vnp_ReturnUrl", fullUrl + "/api/payment/result/vnp");
            vnpay.AddRequestData("vnp_TxnRef", payment.Id); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return paymentUrl;
        }

        private string GetVNPayUrlRefunt(string id, int transactionType, string paymentRefuntId, decimal refuntAmount)
        {
            var _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            string vnp_TmnCodeConfig = _configuration["VNPayConfig:vnp_TmnCode"]; // Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = _configuration["VNPayConfig:vnp_HashSecret"]; // Secret Key

            var vnLib = new VnPayLibrary();

            vnLib.AddRequestData("vnp_RequestId", id);
            vnLib.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnLib.AddRequestData("vnp_Command", "refund");
            vnLib.AddRequestData("vnp_TmnCode", vnp_TmnCodeConfig);

            if (transactionType != 2 && transactionType != 3)
            {
                throw new UserException("Loại giao dịch không phù hợp");
            }

            vnLib.AddRequestData("vnp_TransactionType", transactionType.ToString("00")); // Loại giao dịch tại hệ thống VNPAY
            vnLib.AddRequestData("vnp_TxnRef", paymentRefuntId); // Mã tham chiếu của giao dịch tại hệ thống của merchant

            if (refuntAmount < 0)
            {
                refuntAmount = Math.Abs(refuntAmount);
            }

            var amount = Math.Round(refuntAmount, 2);

            vnLib.AddRequestData("vnp_Amount", ((int)(amount * 100)).ToString()); // Số tiền thanh toán
            vnLib.AddRequestData("vnp_OrderInfo", transactionType == 2 ? $"Giao dich hoan tra toan phan cua giao dich: {paymentRefuntId}" : $"Giao dich hoan tra mot phan cua giao dich: {paymentRefuntId}");
            vnLib.AddRequestData("vnp_TransactionDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"));
            vnLib.AddRequestData("vnp_CreateBy", "ETailor");
            vnLib.AddRequestData("vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"));
            vnLib.AddRequestData("vnp_IpAddr", "20.212.64.6");

            string vnp_SecureHash = vnLib.CreateRefundSecureHash(vnp_HashSecret);
            vnLib.AddRequestData("vnp_SecureHash", vnp_SecureHash);

            return vnLib.GetRequestDataJson();
        }

        public string DemoRefund(string id, int transactionType, string paymentRefuntId, decimal refuntAmount)
        {
            var _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            //Get Config Info
            string vnp_TmnCode = _configuration["VNPayConfig:vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = _configuration["VNPayConfig:vnp_HashSecret"]; //Secret Key

            var vnp_Api = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";

            var vnp_RequestId = id; //Mã hệ thống merchant tự sinh ứng với mỗi yêu cầu hoàn tiền giao dịch. Mã này là duy nhất dùng để phân biệt các yêu cầu truy vấn giao dịch. Không được trùng lặp trong ngày.
            var vnp_Version = VnPayLibrary.VERSION; //2.1.0
            var vnp_Command = "refund";
            var vnp_TransactionType = transactionType.ToString("00");
            var vnp_Amount = Math.Round(Math.Abs(refuntAmount), 2) * 100;
            var vnp_TxnRef = paymentRefuntId; // Mã giao dịch thanh toán tham chiếu
            var vnp_OrderInfo = "Hoan tien giao dich:" + paymentRefuntId;
            var vnp_TransactionNo = ""; //Giả sử giá trị của vnp_TransactionNo không được ghi nhận tại hệ thống của merchant.
            var vnp_TransactionDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
            var vnp_CreateDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
            var vnp_CreateBy = "ETailor";
            var vnp_IpAddr = GetServerIpAddress();

            var signData = vnp_RequestId + "|" + vnp_Version + "|" + vnp_Command + "|" + vnp_TmnCode + "|" + vnp_TransactionType + "|" + vnp_TxnRef + "|" + vnp_Amount + "|" + vnp_TransactionNo + "|" + vnp_TransactionDate + "|" + vnp_CreateBy + "|" + vnp_CreateDate + "|" + vnp_IpAddr + "|" + vnp_OrderInfo;

            var vnp_SecureHash = Ultils.HmacSHA512(vnp_HashSecret, signData);

            var rfData = new
            {
                vnp_RequestId = vnp_RequestId,
                vnp_Version = vnp_Version,
                vnp_Command = vnp_Command,
                vnp_TmnCode = vnp_TmnCode,
                vnp_TransactionType = vnp_TransactionType,
                vnp_TxnRef = vnp_TxnRef,
                vnp_Amount = vnp_Amount,
                vnp_OrderInfo = vnp_OrderInfo,
                vnp_TransactionNo = vnp_TransactionNo,
                vnp_TransactionDate = vnp_TransactionDate,
                vnp_CreateBy = vnp_CreateBy,
                vnp_CreateDate = vnp_CreateDate,
                vnp_IpAddr = vnp_IpAddr,
                vnp_SecureHash = vnp_SecureHash

            };

            var jsonData = JsonConvert.SerializeObject(rfData);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(vnp_Api);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var strData = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                strData = streamReader.ReadToEnd();
            }

            return strData;
        }

        public string GetServerIpAddress()
        {
            string serverIpAddress;
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                serverIpAddress = httpContext?.Connection.LocalIpAddress?.ToString();
            }
            catch (Exception ex)
            {
                serverIpAddress = "Invalid IP:" + ex.Message;
            }

            return serverIpAddress;
        }
    }
}
