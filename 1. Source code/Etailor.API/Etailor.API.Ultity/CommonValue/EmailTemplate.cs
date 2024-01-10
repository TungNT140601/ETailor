using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Ultity.CommonValue
{
    public class EmailOTPTemplate
    {
        private string Username { get; set; }
        private string Otp { get; set; }
        public EmailOTPTemplate(string username, string otp)
        {
            this.Username = username;
            this.Otp = otp;
        }
        public string GetOtpMailBody()
        {
            return "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n  <meta charset=\"UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <title>Your Email Title</title>\r\n  <link href=\"https://fonts.cdnfonts.com/css/agbalumo\" rel=\"stylesheet\">\r\n         <style>\r\n           body {\r\n      font-family: Arial, sans-serif;\r\n      background-color: #f4f4f4;\r\n      margin: 0;\r\n      padding: 0;\r\n    }\r\n\r\n    .container {\r\n      max-width: 600px;\r\n      height: 460px;\r\n      margin: 20px auto;\r\n      background-color: #fff;\r\n      border-radius: 8px;\r\n      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n      overflow: hidden;\r\n    }\r\n\r\n    header {\r\n      color: #000;\r\n      text-align: center;\r\n      padding: 20px 0;\r\n      position: relative;\r\n    }\r\n\r\n    h1 {\r\n      margin: 0;\r\n      font-size: 35px;\r\n    }\r\n\r\n    .content {\r\n      padding: 20px 40px;\r\n      padding-bottom: 0;\r\n      font-size: 16px;\r\n      line-height: 1.6;\r\n      z-index: 1000;\r\n    }\r\n\r\n    footer {\r\n      text-align: center;\r\n      padding: 10px 0;\r\n      background-color: #3498db;\r\n      color: #fff;\r\n    }\r\n.logo-image{\r\n  width: 150px;\r\n  height: 150px;\r\n\r\n  position: absolute;\r\n  left: 0;\r\n  top: -20px;\r\n  border-radius: 50%\r\n}\r\nh3 {\r\n  text-align: center;\r\n  margin-bottom: 30px;\r\n}\r\n\r\n.code{\r\n  background-color: rgba(217, 217, 217, 0.2);\r\n  padding: 20px 60px;\r\n  border: 1px solid #000;\r\n  border-radius: 10px;\r\n  text-align: center;\r\n  letter-spacing: 20px;\r\n  font-size: 30px;\r\n  font-height: bold;\r\n}\r\n.verify-code{\r\n  text-align: center;\r\n}\r\nh1 {\r\n      margin: 0;\r\n      font-size: 35px;\r\n      font-family: Agbalumo, sans-serif;\r\n    }\r\n  </style>\r\n</head>\r\n<body>\r\n  <div class=\"container\">\r\n    \r\n    <header>\r\n      <image src=\"https://drive.google.com/uc?export=view&id=1wT0wV5tdvkVM9L9gkKKddlgbrQggITYn\"\r\n             class=\"logo-image\" />\r\n      <h1 >Nhà may Tuệ</h1>\r\n    </header>\r\n    <hr style=\"width: 500px\">\r\n    <div class=\"content\">\r\n      <p>Hello [Recipient],</p>\r\n      <p>This is a sample email template. You can customize it based on your needs.</p>\r\n      <p>Regards,<br>Your Name</p>\r\n    </div>\r\n    \r\n       <h3> Vertification Code </h3>\r\n    <div class=\"verify-code\">\r\n    <span class=\"code\">123456</span>\r\n    </div>\r\n  </div>\r\n</body>\r\n</html>";
        }
    }
}
