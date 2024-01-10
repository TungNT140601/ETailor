using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Etailor.API.Ultity.CommonValue;

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
                var bodyEmailTemplate = new EmailOTPTemplate("TuDASE151149", "012456");
                message.Body = bodyEmailTemplate.GetOtpMailBody();
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