using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Repository.EntityModels;
using Microsoft.AspNetCore.Builder.Extensions;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Notification = FirebaseAdmin.Messaging.Notification;
using Etailor.API.Ultity;
using Microsoft.Extensions.Configuration;
using Etailor.API.Ultity.PaymentConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Etailor.API.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private string FilePath = "";
        private IConfiguration _configuration;
        public WeatherForecastController(IConfiguration configuration)
        {
            FilePath = Path.Combine(Directory.GetCurrentDirectory(), "userstoken.json"); // Specify your file path
            _configuration = configuration;
        }

        #region SendMail
        [HttpPost]
        public IActionResult SendMail(string body)
        {
            try
            {
                //string fromMail = "tungnt14062001@gmail.com";
                //string fromPassword = "gblfgbilbwaehjkw"; //"tungnt14062001@gmail.com"
                //string fromMail = "tuetailor@gmail.com";
                //string fromPassword = "idpqyvuzktpgstlb"; //"tuetailor@gmail.com"
                string fromMail = "tudase151149@gmail.com";
                string frompassword = "abrxaexoqqpkrjiz"; //"tudase151149@gmail.com"

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "Test Mail " + DateTime.Now.Ticks;
                message.To.Add(new MailAddress("tudase151149@gmail.com"));
                message.Body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <title>Your Email Title</title>\r\n         <style>\r\n           body {\r\n      background-color: #f4f4f4;\r\n      margin: 0;\r\n      padding: 0;\r\n    }\r\n\r\n    .container {\r\n      max-width: 600px;\r\n      height: 460px;\r\n      margin: 20px auto;\r\n      background-color: #fff;\r\n      border-radius: 8px;\r\n      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n      overflow: hidden;\r\n    }\r\n\r\n    header {\r\n      color: #000;\r\n      text-align: center;\r\n      position: relative;\r\n    }\r\n\r\n    h1 {\r\n      margin: 0;\r\n      font-size: 35px;\r\n      font-family: 'Agbalumo', system-ui;\r\n    }\r\n\r\n    .content {\r\n      padding: 20px 40px;\r\n      padding-bottom: 0;\r\n      font-size: 16px;\r\n      line-height: 1.6;\r\n      z-index: 1000;\r\n    }\r\n\r\n    footer {\r\n      text-align: center;\r\n      padding: 10px 0;\r\n      background-color: #3498db;\r\n      color: #fff;\r\n    }\r\n.logo-image{\r\n  width: 130px;\r\n  height: 130px;\r\n  border-radius: 50%\r\n}\r\n\r\n.code{\r\n  background-color: rgba(217, 217, 217, 0.2);\r\n  padding: 20px 60px;\r\n  border: 1px solid #000;\r\n  border-radius: 10px;\r\n  text-align: center;\r\n  letter-spacing: 20px;\r\n  font-size: 30px;\r\n  font-height: bold;\r\n}\r\nh3 {\r\n  text-align: center;\r\n  margin-bottom: 30px;\r\n}\r\n.verify-code{\r\n  text-align: center;\r\n}\r\n@media only screen and (max-width: 600px){\r\n  .code{\r\npadding: 20px 40px;\r\npadding-right: 20px;\r\nletter-spacing: 20px;\r\n\r\n}\r\n}\r\n  </style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n    \r\n  \r\n      <img src=\"https://drive.google.com/uc?export=view&id=1wT0wV5tdvkVM9L9gkKKddlgbrQggITYn\"\r\n             class=\"logo-image\" />\r\n   \r\n    <hr style=\"width: 500px; margin: 0\">\r\n    <div class=\"content\">\r\n      <p>Xin chào <b>";
                message.Body += "";
                message.Body += "</b>,</p>\r\n      <p>Để đảm bảo an toàn cho tài khoản của <b>Quý Khách</b>, vui lòng không chia sẻ mã xác thực này với bất kì ai.</p>\r\n      \r\n    </div>\r\n    \r\n       <h3>Mã xác thực là:</h3>\r\n    <div class=\"verify-code\">\r\n    <span class=\"code\"><b>";
                message.Body += "";
                message.Body += "</b></span>\r\n    </div>\r\n  </div>\r\n</body>\r\n</html>";
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, frompassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        //-----------------------------------------------------------------------

        #region Notification
        [HttpGet]
        public IActionResult AddTokenDevice(string name, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token");
            }
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Invalid name");
            }

            // Check if the token already exists in the loaded tokens
            if (!TokenExists(name, token.Trim(), FilePath))
            {
                // If the token doesn't exist, add it to the file
                AddTokenToFile(name, token, FilePath);
                return Ok("Token added successfully");
            }
            else
            {
                return Ok("Token already exists");
            }
        }

        private bool AddTokenToFile(string name, string token, string filePath)
        {
            try
            {
                // Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    // Create the file if it doesn't exist
                    using (System.IO.File.Create(filePath)) { }
                }

                // Read existing users from the file
                List<UserToken> users = GetTokensFromFile(filePath);

                if (users == null)
                {
                    users = new List<UserToken>();
                }

                // Check if the token already exists
                if (!TokenExists(name, token.Trim(), filePath))
                {
                    // Find the user or create a new one
                    var user = users.FirstOrDefault(u => u.Name == name);
                    if (user == null)
                    {
                        user = new UserToken { Name = name, Tokens = new List<string> { token.Trim() } };
                        users.Add(user);
                    }
                    else
                    {
                        user.Tokens.Add(token.Trim());
                    }

                    // Serialize the updated list of UserToken objects to JSON
                    string jsonContent = JsonConvert.SerializeObject(users);

                    // Write the updated JSON content back to the file
                    using (var writer = new StreamWriter(filePath))
                    {
                        writer.Write(jsonContent);
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine($"Token already exists: {token}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log the exception)
                Console.WriteLine($"Error writing to file: {ex.Message}");
                return false;
            }
        }


        private bool TokenExists(string name, string token, string filePath)
        {
            List<UserToken> users = new List<UserToken>();

            try
            {
                // Check if the file exists before attempting to read it
                if (System.IO.File.Exists(filePath))
                {
                    // Read the existing tokens from the file
                    string jsonContent = System.IO.File.ReadAllText(filePath);

                    // Deserialize the JSON content into a list of TokenEntry objects
                    users = JsonConvert.DeserializeObject<List<UserToken>>(jsonContent);
                    var user = users.Where(c => c.Name == name).FirstOrDefault();
                    if (user != null)
                    {
                        if (user.Tokens.Count > 0 && user.Tokens.Contains(token))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private List<UserToken> GetTokensFromFile(string filePath)
        {
            List<UserToken> users = new List<UserToken>();

            try
            {
                // Check if the file exists before attempting to read it
                if (System.IO.File.Exists(filePath))
                {
                    // Read the existing tokens from the file
                    string jsonContent = System.IO.File.ReadAllText(filePath);

                    // Deserialize the JSON content into a list of TokenEntry objects
                    users = JsonConvert.DeserializeObject<List<UserToken>>(jsonContent);

                    return users;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
        private List<UserToken> GetUserTokensFromFile(string name, string filePath)
        {
            List<UserToken> users = new List<UserToken>();

            try
            {
                // Check if the file exists before attempting to read it
                if (System.IO.File.Exists(filePath))
                {
                    // Read the existing tokens from the file
                    string jsonContent = System.IO.File.ReadAllText(filePath);

                    // Deserialize the JSON content into a list of TokenEntry objects
                    users = JsonConvert.DeserializeObject<List<UserToken>>(jsonContent);

                    return users.Where(c => c.Name == name).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotifyAsync([FromBody] Notify notify)
        {
            List<UserToken> tokens = new List<UserToken>();

            if (string.IsNullOrWhiteSpace(notify.Name))
            {
                tokens = GetTokensFromFile(FilePath);
            }
            else
            {
                tokens = GetUserTokensFromFile(notify.Name, FilePath);
            }

            // Extracting token values from the list of dictionaries
            List<string> tokenValues = new List<string>();

            foreach (var token in tokens)
            {
                tokenValues.AddRange(token.Tokens);
            }

            // Remove null or empty tokens
            tokenValues.RemoveAll(string.IsNullOrEmpty);

            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = notify.Title,
                    Body = notify.Body,
                },
                Tokens = tokenValues
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
                return Ok($"Successfully sent message: {response}");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Failed to send notification: {ex.Message}");
                return StatusCode(500, "Failed to send notification");
            }
        }

        #endregion

        //-----------------------------------------------------------------------

        #region Payment
        [HttpGet]
        public string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        [HttpGet]
        public IActionResult GetUrlPayment(int amout)
        {
            //Get Config Info
            string vnp_Returnurl = _configuration.GetValue<string>("VNPayConfig:vnp_Returnurl"); //URL nhan ket qua tra ve 
            string vnp_Url = _configuration.GetValue<string>("VNPayConfig:vnp_Url"); //URL thanh toan cua VNPAY 
            string vnp_TmnCode = _configuration.GetValue<string>("VNPayConfig:vnp_TmnCode"); //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = _configuration.GetValue<string>("VNPayConfig:vnp_HashSecret"); //Secret Key

            //Get payment input
            //OrderInfo order = new OrderInfo();
            //order.OrderId = DateTime.Now.Ticks; // Giả lập mã giao dịch hệ thống merchant gửi sang VNPAY
            //order.Amount = 100000; // Giả lập số tiền thanh toán hệ thống merchant gửi sang VNPAY 100,000 VND
            //order.Status = "0"; //0: Trạng thái thanh toán "chờ thanh toán" hoặc "Pending" khởi tạo giao dịch chưa có IPN
            //order.CreatedDate = DateTime.Now;
            //Save order to db

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (amout * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_BankCode", "");

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", GetIpAddress());

            var orderId = Ultils.GenGuidString();

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + orderId);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", orderId); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Redirect(paymentUrl);
        }

        [HttpGet]
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

            //return Ok(responseData);
            return Redirect("https://demo-notification.vercel.app");
        }

        #endregion

        [HttpGet]
        [Route("downloadfile")]
        public IActionResult DownloadFile()
        {

            if (!System.IO.File.Exists(FilePath))
            {
                return NotFound("File not found");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(FilePath);
            return File(fileBytes, "application/octet-stream", Path.GetFileName(FilePath));
        }
    }
    public class Notify
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
    public class UserToken
    {
        public string Name { get; set; }
        public List<string> Tokens { get; set; }
    }
}