using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace Etailor.API.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpPost]
        public IActionResult SendMail([FromBody] SendMailModel sendMailModel)
        {
            try
            {

                string fromMail = "tuetailor@gmail.com";
                //string fromPassword = "gblfgbilbwaehjkw"; //"tungnt14062001@gmail.com"
                string fromPassword = "idpqyvuzktpgstlb"; //"tuetailor@gmail.com"

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = sendMailModel.Subject;
                foreach (var mail in sendMailModel.Emails)
                {
                    message.To.Add(new MailAddress(mail));
                }
                message.Body = "<!DOCTYPE html>" +
                    "<html lang=\"en\">\r\n<head>\r\n " +
                    "<meta charset=\"UTF-8\">\r\n  " +
                    "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  " +
                    "<title>Your Email Title</title>\r\n  " +
                    "<style>\r\n    " +
                    "body {\r\n      " +
                    "font-family: Arial, sans-serif;\r\n      " +
                    "background-color: #f4f4f4;\r\n      " +
                    "margin: 0;\r\n      " +
                    "padding: 0;\r\n    }\r\n\r\n    " +
                    ".container {\r\n      " +
                    "max-width: 600px;\r\n      " +
                    "margin: 20px auto;\r\n      " +
                    "background-color: #fff;\r\n      " +
                    "border-radius: 8px;\r\n      " +
                    "box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n      " +
                    "overflow: hidden;\r\n    }\r\n\r\n    header {\r\n      " +
                    "background-color: #3498db;\r\n      color: #fff;\r\n      " +
                    "text-align: center;\r\n      padding: 20px 0;\r\n    }\r\n\r\n    " +
                    "h1 {\r\n      margin: 0;\r\n      font-size: 24px;\r\n    }\r\n\r\n    " +
                    ".content {\r\n      padding: 20px;\r\n      font-size: 16px;\r\n      " +
                    "line-height: 1.6;\r\n    }\r\n\r\n    footer {\r\n      text-align: center;\r\n      " +
                    "padding: 10px 0;\r\n      background-color: #3498db;\r\n      color: #fff;\r\n    }\r\n  " +
                    "</style>\r\n</head>\r\n" +
                    "<body>\r\n  <div class=\"container\">\r\n    <header>\r\n      " +
                    "<h1>Your Company Name</h1>\r\n    </header>\r\n    " +
                    "<div class=\"content\">\r\n      " +
                    "<p>Hello [Recipient],</p>\r\n      " +
                    "<p>";

                message.Body += sendMailModel.Body;
                message.Body += "</p>\\r\\n      \" +\r\n                    " +
                    "\"<p>Regards,<br>Your Name</p>\\r\\n    \" +\r\n                    " +
                    "\"</div>\\r\\n    \" +\r\n                    \"" +
                    "<footer>\\r\\n      &copy; 2024 Your Company. All rights reserved.\\r\\n    </footer>\\r\\n  \" +\r\n                    \"" +
                    "</div>\\r\\n</body>\\r\\n</html>\"";
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
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
    }
    public class SendMailModel
    {
        public List<string> Emails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}